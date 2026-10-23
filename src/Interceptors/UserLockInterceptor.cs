using Grpc.Core;
using Grpc.Core.Interceptors;
using MariesWonderland.Data;
using MariesWonderland.Extensions;

namespace MariesWonderland.Interceptors;

/// <summary>
/// gRPC interceptor that runs every logged-in call under its caller's lock, so one user's requests never change
/// their data at the same time while different users run in parallel. It sits right after
/// <see cref="SessionInterceptor"/>, so the diff and the request journal are built inside the lock too.
/// Calls without a session (RegisterUser, Auth…) aren't locked here; they lock the user they touch themselves.
/// </summary>
public class UserLockInterceptor(UserDataStore store) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        if (!context.TryGetUserId(out long userId))
        {
            return await continuation(request, context);
        }

        using IDisposable userLock = await store.Locks.AcquireAsync(userId, context.CancellationToken);
        context.SetLockedUserId(userId);
        return await continuation(request, context);
    }
}
