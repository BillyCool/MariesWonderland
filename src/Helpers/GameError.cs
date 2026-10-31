using Grpc.Core;

namespace MariesWonderland.Helpers;

/// <summary>What the client does after an error dialog (<c>x-apb-screen-transition-type</c>).</summary>
public enum ScreenTransition
{
    /// <summary>OK dialog, the player stays where they are.</summary>
    Stay = 1,

    /// <summary>Dialog, then back to the top screen of the current feature.</summary>
    FunctionTop = 2,

    /// <summary>Dialog, then back to the title screen.</summary>
    Title = 3,
}

/// <summary>
/// Every error code the client knows: <c>nt.Network.&lt;code&gt;</c> text keys, the codes its network layer checks
/// (<c>HandleNet</c>, <c>InterceptorSettings</c>) and the ones its purchase flow checks (<c>DarkPurchase</c>). Sent as
/// the same number in <c>x-apb-message-code</c> and <c>x-apb-message-text-id</c>. Texts and behaviour:
/// docs/systems/headers-and-errors.md.
/// </summary>
public static class NetworkCode
{
    // System: maintenance, ban, update, periods.

    /// <summary>Full maintenance; the dialog shows <c>x-apb-message-text</c>, then the title.</summary>
    public const int MaintenanceAll = 100000;

    /// <summary>Partial maintenance; the dialog shows the end time from <c>x-apb-message-datetime</c>.</summary>
    public const int MaintenancePart = 100001;

    /// <summary>Opens a web view at the page path in <c>x-apb-message-text</c>.</summary>
    public const int MaintenanceWebView = 100002;

    public const int ForbiddenWord = 101003;

    /// <summary>"Unauthorized play data, use of this app is suspended", then the contact page.</summary>
    public const int UserBanned = 102002;

    /// <summary>"This play data is exclusively for CBT use only" (no Japanese text).</summary>
    public const int UserCbtBanned = 1020013;

    /// <summary>"A new version is available", then the store page.</summary>
    public const int ForceUpdate = 103000;

    public const int ReturningToTitle = 103001;

    /// <summary>New master data: "New data updated. Returning to title screen." with the "Refresh" title.</summary>
    public const int MasterDataUpdated = 104000;

    /// <summary>"Time limit exceeded. Returning to top."</summary>
    public const int OutOfPeriod = 105000;

    public const int EventPeriodOver = 105001;

    // Feature errors.

    public const int UserNameChangeTooSoon = 200011;
    public const int CommentChangeTooSoon = 200012;
    public const int FavoriteChangeTooSoon = 200013;
    public const int NotEnoughStamina = 200018;
    public const int StaminaFull = 200019;
    public const int NotEnoughBattlePoints = 200021;
    public const int BattlePointsFull = 200022;

    /// <summary>"Updating item shop." (the shop's stock changed)</summary>
    public const int ShopUpdated = 201001;

    public const int CannotCarryMoreOfItem = 201002;

    /// <summary>Also a purchase code: <c>PurchaseDuplicateError</c> when creating the transaction.</summary>
    public const int AlreadyPurchased = 201003;

    /// <summary>Like <see cref="AlreadyPurchased"/> plus "make an inquiry"; <c>PurchaseDuplicateError</c> in receipt checks.</summary>
    public const int AlreadyPurchasedInquire = 201100;

    public const int QuestNotFound = 202001;

    /// <summary>English says "already completed"; the Japanese says "already started".</summary>
    public const int QuestAlreadyStarted = 202002;

    public const int QuestNotUnlocked = 202004;
    public const int QuestDailyLimit = 202005;
    public const int GiftsNotObtained = 203001;
    public const int AlreadySentFriendRequest = 204001;
    public const int FriendLimit = 204002;
    public const int AlreadyFriends = 204003;
    public const int FriendNotFound = 204004;
    public const int CheerAlreadySent = 204005;
    public const int FriendRequestNotFound = 204006;
    public const int CheerAlreadyReceived = 204007;
    public const int FriendRequestBoxFull = 204008;

    /// <summary>Same text as <see cref="FriendLimit"/>; probably the other player's limit.</summary>
    public const int TheirFriendLimit = 204009;

    public const int CheerSendDayLimit = 204010;
    public const int CheerReceiveDayLimit = 204011;
    public const int CharacterNotInLoadout = 205001;

    /// <summary>"This wave cannot be ended as your deck loadout information has changed…"</summary>
    public const int DeckChangedDuringBattle = 206099;

    // Traffic, account and purchase.

    public const int HeavyTraffic = 290000;

    /// <summary>No text. Purchase flow: <c>PurchaseSystemError</c>.</summary>
    public const int PurchaseSystemError1 = 290001;

    /// <summary>No text. Purchase flow: <c>PurchaseSystemError</c>.</summary>
    public const int PurchaseSystemError2 = 290002;

    /// <summary>Same ban text as <see cref="UserBanned"/>, but a normal dialog, not the special ban flow.</summary>
    public const int UserBannedDialog = 290021;

    public const int CreatePlayerFailed = 290024;
    public const int NotEnoughGems = 290030;

    /// <summary>No text. Purchase flow: <c>PurchaseSystemError</c>.</summary>
    public const int PurchaseSystemError3 = 290040;

    /// <summary>No text. The error handler ignores it; a receipt check counts it as success (receipt already applied).</summary>
    public const int ReceiptAlreadyApplied = 290041;

    /// <summary>No text. Purchase flow: <c>PurchaseSystemError</c> in receipt checks.</summary>
    public const int PurchaseSystemError4 = 290042;

    /// <summary>"You have reached your spending cap." The error handler ignores it; the purchase flow shows its own dialog.</summary>
    public const int SpendingCap = 290043;

    /// <summary>No <c>nt.Network</c> text. The error handler ignores it; the purchase flow shows the "too many refunds" dialog.</summary>
    public const int TooManyRefunds = 290044;

    // Other.

    public const int SerialCodeInvalid = 300000;
    public const int SerialCodeUsed = 300001;
    public const int NoNotices = 400000;

    /// <summary>"Process failed. Returning to title screen." Also the fallback text for a Title transition.</summary>
    public const int ProcessFailedToTitle = 900000;

    /// <summary>"Failed to connect. Retrying." The client's own retry text.</summary>
    public const int ConnectRetrying = 900001;

    /// <summary>"Process failed.", the client's generic text for a refusal with no text of its own.</summary>
    public const int ProcessFailed = 900002;
}

/// <summary>Builds the failed-call exceptions that make the client show its own dialog.</summary>
public static class GameError
{
    /// <summary>
    /// A refusal. <paramref name="message"/> is for our logs; the player sees the client's text for <paramref name="code"/>.
    /// </summary>
    public static RpcException Refuse(string message, int code = NetworkCode.ProcessFailed, ScreenTransition transition = ScreenTransition.Stay, StatusCode status = StatusCode.FailedPrecondition)
    {
        Metadata trailers = new()
        {
            { "x-apb-screen-transition-type", ((int)transition).ToString() },
            { "x-apb-message-code", code.ToString() },
            { "x-apb-message-text-id", code.ToString() },
        };

        return new RpcException(new Status(status, message), trailers);
    }
}
