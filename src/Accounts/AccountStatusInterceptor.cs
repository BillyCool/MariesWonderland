using Grpc.Core;
using Grpc.Core.Interceptors;
using MariesWonderland.Constants;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;

namespace MariesWonderland.Accounts;

/// <summary>
/// Stops every game call from a banned or disabled account (right after <c>SessionInterceptor</c> resolved the user), so
/// a player who is playing when the administrator acts gets the dialog on their next call. Their sessions stay, on
/// purpose: a call without a session would fail as signed out, without the dialog. <c>Auth</c> and <c>TransferUser</c>
/// run before there is a session and refuse with <see cref="Refusal"/> themselves.
/// </summary>
public sealed class AccountStatusInterceptor(AccountStore accounts) : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        if (context.TryGetUserId(out long userId) && Refusal(accounts.BlockOf(userId)) is { } refusal)
        {
            throw refusal;
        }

        return continuation(request, context);
    }

    /// <summary>
    /// What a blocked account gets, or null when it may play. Banned: the game's ban code (102002), which the client
    /// special-cases: its ban dialog ("We have identified unauthorized play data. Use of this app is now suspended…"),
    /// then its contact page. Disabled: "Process failed. Returning to title screen." (900000).
    /// </summary>
    public static RpcException? Refusal(AccountBlock block) => block switch
    {
        AccountBlock.Banned => GameError.Refuse(SystemErrors.AccountBanned, NetworkCode.UserBanned, ScreenTransition.Title, StatusCode.PermissionDenied),
        AccountBlock.Disabled => GameError.Refuse(SystemErrors.AccountDisabled, NetworkCode.ProcessFailedToTitle, ScreenTransition.Title, StatusCode.PermissionDenied),
        _ => null,
    };
}
