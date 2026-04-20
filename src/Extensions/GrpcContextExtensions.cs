using Grpc.Core;

namespace MariesWonderland.Extensions;

/// <summary>
/// Extension methods for extracting client-provided headers from gRPC <see cref="ServerCallContext"/>.
/// The client sends userId and session key as custom metadata entries.
/// </summary>
public static class GrpcContextExtensions
{
    /// <summary>
    /// Reads the caller's user ID from the <c>x-apb-user-id</c> request header.
    /// </summary>
    public static long GetUserId(this ServerCallContext context)
    {
        string? value = context.RequestHeaders.GetValue("x-apb-user-id");
        return value != null && long.TryParse(value, out long id) ? id : 0;
    }

    /// <summary>
    /// Reads the caller's session key from the <c>x-apb-session-key</c> request header.
    /// </summary>
    public static string GetSessionKey(this ServerCallContext context)
        => context.RequestHeaders.GetValue("x-apb-session-key") ?? "";
}
