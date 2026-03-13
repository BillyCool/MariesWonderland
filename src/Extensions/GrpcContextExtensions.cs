using Grpc.Core;

namespace MariesWonderland.Extensions;

public static class GrpcContextExtensions
{
    public static long GetUserId(this ServerCallContext context)
    {
        string? value = context.RequestHeaders.GetValue("x-apb-user-id");
        return value != null && long.TryParse(value, out long id) ? id : 0;
    }

    public static string GetSessionKey(this ServerCallContext context)
        => context.RequestHeaders.GetValue("x-apb-session-key") ?? "";
}
