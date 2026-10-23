using Grpc.Core;
using MariesWonderland.Data;

namespace MariesWonderland.Extensions;

/// <summary>
/// Extension methods for the caller's identity on a gRPC <see cref="ServerCallContext"/>.
/// The identity is resolved from the session key by <c>SessionInterceptor</c>; the client's own
/// <c>x-apb-user-id</c> header is never trusted.
/// </summary>
public static class GrpcContextExtensions
{
    private const string UserIdKey = "MariesWonderland.UserId";
    private const string LockedUserIdKey = "MariesWonderland.LockedUserId";
    private const string RequestIdKey = "MariesWonderland.RequestId";
    private const string UserChangesKey = "MariesWonderland.UserChanges";

    /// <summary>A short id for this call, made on first use, shared by the gRPC log and the request journal.</summary>
    public static string GetRequestId(this ServerCallContext context)
    {
        if (context.UserState.TryGetValue(RequestIdKey, out object? value) && value is string id)
        {
            return id;
        }

        string newId = Guid.NewGuid().ToString("N")[..8];
        context.UserState[RequestIdKey] = newId;
        return newId;
    }

    /// <summary>Records what this call changed in a user's data (set where the changes are saved).</summary>
    public static void SetUserChanges(this ServerCallContext context, UserChanges changes)
        => context.UserState[UserChangesKey] = changes;

    /// <summary>What this call changed in a user's data, if it recorded it.</summary>
    public static UserChanges? GetUserChanges(this ServerCallContext context)
        => context.UserState.TryGetValue(UserChangesKey, out object? value) ? value as UserChanges : null;

    /// <summary>
    /// Returns the authenticated caller's user ID.
    /// Throws <see cref="StatusCode.Unauthenticated"/> when the call has no valid session.
    /// </summary>
    public static long GetUserId(this ServerCallContext context)
        => context.TryGetUserId(out long userId)
            ? userId
            : throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing or expired session."));

    /// <summary>Returns the authenticated caller's user ID, if the call has a valid session.</summary>
    public static bool TryGetUserId(this ServerCallContext context, out long userId)
    {
        if (context.UserState.TryGetValue(UserIdKey, out object? value) && value is long id)
        {
            userId = id;
            return true;
        }

        userId = 0;
        return false;
    }

    /// <summary>Records the authenticated caller's user ID on the call.</summary>
    public static void SetUserId(this ServerCallContext context, long userId)
        => context.UserState[UserIdKey] = userId;

    /// <summary>
    /// Reads the caller's session key from the <c>x-apb-session-key</c> request header.
    /// </summary>
    public static string GetSessionKey(this ServerCallContext context)
        => context.RequestHeaders.GetValue("x-apb-session-key") ?? "";

    /// <summary>Records that this call holds <paramref name="userId"/>'s lock (set by <c>UserLockInterceptor</c>).</summary>
    public static void SetLockedUserId(this ServerCallContext context, long userId)
        => context.UserState[LockedUserIdKey] = userId;

    /// <summary>
    /// Takes <paramref name="userId"/>'s lock for the rest of this call, waiting up to
    /// <see cref="UserDataStore.LockTimeout"/>; null when it stayed busy. Returns a no-op when this call already
    /// holds that user's lock (its own caller), since the locks aren't reentrant.
    /// </summary>
    public static IDisposable? TryLockUser(this ServerCallContext context, UserDataStore store, long userId)
    {
        if (context.UserState.TryGetValue(LockedUserIdKey, out object? value) && value is long held && held == userId)
        {
            return NoLock.Instance;
        }

        return store.Locks.TryAcquire(userId, UserDataStore.LockTimeout);
    }

    /// <summary>Like <see cref="TryLockUser"/>, but fails the call with <see cref="StatusCode.Unavailable"/> when the user stayed busy.</summary>
    public static IDisposable LockUser(this ServerCallContext context, UserDataStore store, long userId)
        => context.TryLockUser(store, userId)
            ?? throw new RpcException(new Status(StatusCode.Unavailable, "User is busy, try again."));

    private sealed class NoLock : IDisposable
    {
        public static readonly NoLock Instance = new();

        public void Dispose()
        {
        }
    }
}
