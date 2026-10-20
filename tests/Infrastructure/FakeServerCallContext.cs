using Grpc.Core;
using MariesWonderland.Extensions;

namespace MariesWonderland.Tests.Infrastructure;

/// <summary>
/// Minimal <see cref="ServerCallContext"/> stub for calling services and interceptors without a real
/// gRPC channel. <see cref="For"/> marks the call as authenticated, as <c>SessionInterceptor</c> would,
/// so service methods can call <c>context.GetUserId()</c>. The call is cancelled with the current test.
/// </summary>
public sealed class FakeServerCallContext : ServerCallContext
{
    private readonly Metadata _requestHeaders = [];
    private readonly Metadata _responseTrailers = [];

    private FakeServerCallContext(string? sessionKey)
    {
        if (!string.IsNullOrEmpty(sessionKey))
        {
            _requestHeaders.Add("x-apb-session-key", sessionKey);
        }
    }

    /// <summary>
    /// A call authenticated as <paramref name="userId"/>. A userId of 0 means no session,
    /// like the calls made before login.
    /// </summary>
    public static FakeServerCallContext For(long userId, string? sessionKey = null)
    {
        FakeServerCallContext context = new(sessionKey);
        if (userId != 0)
        {
            context.SetUserId(userId);
        }
        return context;
    }

    /// <summary>A call with no resolved identity, optionally carrying extra request headers.</summary>
    public static FakeServerCallContext Anonymous(string? sessionKey = null, params Metadata.Entry[] headers)
    {
        FakeServerCallContext context = new(sessionKey);
        foreach (Metadata.Entry header in headers)
        {
            context._requestHeaders.Add(header);
        }
        return context;
    }

    protected override string MethodCore => string.Empty;
    protected override string HostCore => string.Empty;
    protected override string PeerCore => string.Empty;
    protected override DateTime DeadlineCore => DateTime.MaxValue;
    protected override Metadata RequestHeadersCore => _requestHeaders;
    protected override CancellationToken CancellationTokenCore => TestContext.Current.CancellationToken;
    protected override Metadata ResponseTrailersCore => _responseTrailers;
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore => new(string.Empty, []);

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) =>
        throw new NotImplementedException();

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) =>
        throw new NotImplementedException();
}
