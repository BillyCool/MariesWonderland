using Grpc.Core;

namespace MariesWonderland.Extensions;

/// <summary>
/// Extension methods for the caller's identity on a gRPC <see cref="ServerCallContext"/>.
/// The identity is resolved from the session key by <c>SessionInterceptor</c>; the client's own
/// <c>x-apb-user-id</c> header is never trusted.
/// </summary>
public static class GrpcContextExtensions
{
    private const string UserIdKey = "MariesWonderland.UserId";

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
}
