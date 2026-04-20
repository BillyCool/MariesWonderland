using Grpc.Core;
using Grpc.Core.Interceptors;

namespace MariesWonderland.Interceptors;

/// <summary>
/// gRPC interceptor that appends standard response metadata to every call.
/// Currently adds the <c>x-apb-response-datetime</c> trailer with the server's UTC timestamp
/// in Unix milliseconds, which the client uses for time synchronisation.
/// </summary>
public class CommonHeaderInterceptor : Interceptor
{
    /// <summary>
    /// Runs after the service handler completes and appends the response-datetime trailer.
    /// </summary>
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        TResponse response = await continuation(request, context);

        Metadata trailers = context.ResponseTrailers;
        trailers.Add("x-apb-response-datetime", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());

        return response;
    }
}
