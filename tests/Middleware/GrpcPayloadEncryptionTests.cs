using System.Buffers;
using System.Buffers.Binary;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using MariesWonderland.Middleware;
using MariesWonderland.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MariesWonderland.Tests.Middleware;

/// <summary>
/// The gRPC message encryption: the cipher and frame handling on their own, then a call through the real gRPC server
/// over HTTP/2 (<see cref="ConfigService"/>, which needs no data) to show the services behind the middleware stay plain.
/// </summary>
public sealed class GrpcPayloadEncryptionTests : IAsyncLifetime
{
    private const string Call = "/apb.api.config.ConfigService/GetReviewServerConfig";

    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        string root = Path.Combine(Path.GetTempPath(), $"mw-grpc-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = root, EnvironmentName = "Production" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.WebHost.ConfigureKestrel(options => options.ConfigureEndpointDefaults(endpoint => endpoint.Protocols = HttpProtocols.Http2));
        builder.Services.AddGrpc();
        _app = builder.Build();
        _app.UseMiddleware<GrpcPayloadEncryptionMiddleware>();
        _app.MapGrpcService<ConfigService>();
        _app.MapGet("/", () => "plain");
        await _app.StartAsync();

        _client = new HttpClient
        {
            BaseAddress = new Uri(_app.Urls.First()),
            DefaultRequestVersion = HttpVersion.Version20,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
        };
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public void Encrypt_MatchesTheClientsAesSetup()
    {
        byte[] plain = Encoding.UTF8.GetBytes("a protobuf message, more or less");

        using Aes client = Aes.Create();
        client.Mode = CipherMode.CBC;
        client.Key = Encoding.UTF8.GetBytes("1234567890ABCDEF");
        client.IV = Encoding.UTF8.GetBytes("it8bAjktKdFIBYtU");
        byte[] expected = client.CreateEncryptor().TransformFinalBlock(plain, 0, plain.Length);

        Assert.Equal(expected, GrpcPayloadCipher.Encrypt(plain));
        Assert.Equal(plain, GrpcPayloadCipher.Decrypt(expected));
    }

    [Fact]
    public void Encrypt_EmptyMessage_IsOnePaddingBlock()
    {
        byte[] cipher = GrpcPayloadCipher.Encrypt([]);

        Assert.Equal(16, cipher.Length);
        Assert.Empty(GrpcPayloadCipher.Decrypt(cipher));
    }

    [Fact]
    public void TransformFrames_RoundTripsEveryFrameAndLeavesAPartialOne()
    {
        byte[][] messages = [[1, 2, 3], [], [.. Enumerable.Range(0, 100).Select(i => (byte)i)]];
        ArrayBufferWriter<byte> plainFrames = new();
        foreach (byte[] message in messages)
        {
            WriteFrame(plainFrames, message);
        }

        byte[] partial = [0, 0, 0, 0, 9, 1];
        byte[] input = [.. plainFrames.WrittenSpan, .. partial];

        ArrayBufferWriter<byte> encrypted = new();
        int consumed = GrpcPayloadCipher.TransformFrames(input, encrypted, encrypt: true);
        Assert.Equal(plainFrames.WrittenCount, consumed);

        ArrayBufferWriter<byte> decrypted = new();
        Assert.Equal(encrypted.WrittenCount, GrpcPayloadCipher.TransformFrames(encrypted.WrittenSpan, decrypted, encrypt: false));
        Assert.Equal(plainFrames.WrittenSpan.ToArray(), decrypted.WrittenSpan.ToArray());
    }

    [Fact]
    public void TransformFrames_CompressedFrame_Throws()
    {
        byte[] frame = [1, 0, 0, 0, 0];

        Assert.Throws<InvalidDataException>(() => GrpcPayloadCipher.TransformFrames(frame, new ArrayBufferWriter<byte>(), encrypt: false));
    }

    [Fact]
    public void TransformFrames_PlainMessageToDecrypt_Throws()
    {
        ArrayBufferWriter<byte> plain = new();
        WriteFrame(plain, [1, 2, 3, 4, 5]);

        Assert.ThrowsAny<CryptographicException>(() => GrpcPayloadCipher.TransformFrames(plain.WrittenSpan, new ArrayBufferWriter<byte>(), encrypt: false));
    }

    [Fact]
    public async Task EncryptedCall_ReachesTheServiceAndComesBackEncrypted()
    {
        ArrayBufferWriter<byte> request = new();
        WriteFrame(request, GrpcPayloadCipher.Encrypt([]));

        using HttpResponseMessage response = await Post(request.WrittenMemory.ToArray());
        byte[] body = await response.Content.ReadAsByteArrayAsync();

        Assert.Equal("0", GrpcStatus(response));
        Assert.Equal(0, body[0]);
        Assert.Equal(16u, BinaryPrimitives.ReadUInt32BigEndian(body.AsSpan(1)));
        Assert.Equal(21, body.Length);
        Assert.Empty(GrpcPayloadCipher.Decrypt(body.AsSpan(5)));
    }

    [Fact]
    public async Task PlainCall_IsRejectedAsInvalidArgument()
    {
        ArrayBufferWriter<byte> request = new();
        WriteFrame(request, []);

        using HttpResponseMessage response = await Post(request.WrittenMemory.ToArray());

        Assert.Equal("3", GrpcStatus(response));
    }

    [Fact]
    public async Task NonGrpcRequest_PassesThrough()
    {
        using HttpClient plain = new() { BaseAddress = _client.BaseAddress, DefaultRequestVersion = HttpVersion.Version20, DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact };

        Assert.Equal("plain", await plain.GetStringAsync("/"));
    }

    private async Task<HttpResponseMessage> Post(byte[] body)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, Call)
        {
            Content = new ByteArrayContent(body),
            Version = HttpVersion.Version20,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact,
        };
        request.Content.Headers.ContentType = new("application/grpc");
        request.Headers.TryAddWithoutValidation("te", "trailers");

        HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        await response.Content.LoadIntoBufferAsync();
        return response;
    }

    /// <summary>The call's status: in the trailers, or in the headers when the response is trailers-only.</summary>
    private static string GrpcStatus(HttpResponseMessage response)
        => response.TrailingHeaders.Concat(response.Headers).Single(header => header.Key == "grpc-status").Value.Single();

    private static void WriteFrame(IBufferWriter<byte> output, ReadOnlySpan<byte> message)
    {
        Span<byte> span = output.GetSpan(5 + message.Length);
        span[0] = 0;
        BinaryPrimitives.WriteUInt32BigEndian(span[1..], (uint)message.Length);
        message.CopyTo(span[5..]);
        output.Advance(5 + message.Length);
    }
}
