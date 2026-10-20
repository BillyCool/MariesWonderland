using Grpc.Core;
using Grpc.Core.Interceptors;
using MariesWonderland.Data;
using MariesWonderland.Extensions;

namespace MariesWonderland.Interceptors;

/// <summary>
/// gRPC interceptor that resolves the caller's identity from the <c>x-apb-session-key</c> header
/// issued by <c>Auth</c>, and stores it on the call for <see cref="GrpcContextExtensions.GetUserId"/>.
/// It never rejects a call itself: calls made before login (RegisterUser, Auth, TransferUser…) carry
/// no valid session and don't need one. Handlers that need a user fail with Unauthenticated when
/// no session was resolved.
/// </summary>
public class SessionInterceptor(UserDataStore store) : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        string sessionKey = context.GetSessionKey();
        if (sessionKey.Length > 0 && store.TryResolveSession(sessionKey, out long userId))
        {
            context.SetUserId(userId);
        }

        return continuation(request, context);
    }
}
