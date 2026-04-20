using Grpc.Core;

namespace MariesWonderland.Tests.Infrastructure;

/// <summary>
/// Minimal <see cref="ServerCallContext"/> stub that pre-populates the
/// <c>x-apb-user-id</c> request header so service methods can call
/// <c>context.GetUserId()</c> without a real gRPC channel.
/// </summary>
public sealed class FakeServerCallContext : ServerCallContext
{
    private readonly Metadata _requestHeaders;
    private readonly Metadata _responseTrailers = [];
    private readonly CancellationToken _cancellationToken;

    private FakeServerCallContext(long userId, CancellationToken cancellationToken = default)
    {
        _requestHeaders = [new Metadata.Entry("x-apb-user-id", userId.ToString())];
        _cancellationToken = cancellationToken;
    }

    public static FakeServerCallContext For(long userId, CancellationToken cancellationToken = default)
        => new(userId, cancellationToken);

    protected override string MethodCore => string.Empty;
    protected override string HostCore => string.Empty;
    protected override string PeerCore => string.Empty;
    protected override DateTime DeadlineCore => DateTime.MaxValue;
    protected override Metadata RequestHeadersCore => _requestHeaders;
    protected override CancellationToken CancellationTokenCore => _cancellationToken;
    protected override Metadata ResponseTrailersCore => _responseTrailers;
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore => new(string.Empty, []);

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) =>
        throw new NotImplementedException();

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) =>
        throw new NotImplementedException();
}
