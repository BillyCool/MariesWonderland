using System.Buffers;
using System.Security.Cryptography;
using Grpc.Core;
using Microsoft.AspNetCore.Http.Features;

namespace MariesWonderland.Middleware;

/// <summary>
/// Decrypts the messages of gRPC requests and encrypts those of responses (see <see cref="GrpcPayloadCipher"/>), so
/// the gRPC pipeline behind it, interceptors and services included, only sees plain messages. It sits in front of
/// the gRPC server because interceptors get messages already parsed, which an encrypted body can't be.
/// The request is read whole before the call starts, which suits the unary calls the client makes.
/// </summary>
public class GrpcPayloadEncryptionMiddleware(RequestDelegate next, ILogger<GrpcPayloadEncryptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsGrpc(context.Request))
        {
            await next(context);
            return;
        }

        ArrayBufferWriter<byte> plain = new();
        try
        {
            using MemoryStream body = new();
            await context.Request.Body.CopyToAsync(body, context.RequestAborted);
            int consumed = GrpcPayloadCipher.TransformFrames(body.GetBuffer().AsSpan(0, (int)body.Length), plain, encrypt: false);
            if (consumed != body.Length)
            {
                throw new InvalidDataException("The request ends with a partial gRPC frame.");
            }
        }
        catch (Exception exception) when (exception is CryptographicException or InvalidDataException)
        {
            logger.LogWarning(exception, "Rejected {Path}: the request isn't a validly encrypted gRPC message", context.Request.Path);
            Reject(context.Response);
            return;
        }

        context.Request.Body = new MemoryStream(plain.WrittenMemory.ToArray());
        context.Features.Set<IRequestBodyPipeFeature>(new RequestBodyPipeFeature(context));

        IHttpResponseBodyFeature original = context.Features.GetRequiredFeature<IHttpResponseBodyFeature>();
        EncryptingResponseBodyFeature encrypting = new(original);
        context.Features.Set<IHttpResponseBodyFeature>(encrypting);
        try
        {
            await next(context);
            encrypting.Transfer();
        }
        finally
        {
            context.Features.Set(original);
        }
    }

    private static bool IsGrpc(HttpRequest request)
        => request.ContentType is string contentType
            && contentType.StartsWith("application/grpc", StringComparison.OrdinalIgnoreCase)
            && !contentType.StartsWith("application/grpc-web", StringComparison.OrdinalIgnoreCase);

    /// <summary>Answers with a trailers-only gRPC response, which is how a call fails without a message.</summary>
    private static void Reject(HttpResponse response)
    {
        response.StatusCode = StatusCodes.Status200OK;
        response.ContentType = "application/grpc";
        response.Headers["grpc-status"] = ((int)StatusCode.InvalidArgument).ToString();
        response.Headers["grpc-message"] = "Invalid message encryption";
    }
}
