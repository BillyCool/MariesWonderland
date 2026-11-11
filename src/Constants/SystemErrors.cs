namespace MariesWonderland.Constants;

public static class SystemErrors
{
    public const string MasterDataOutOfDate = "Master data is out of date";
    public const string MissingSession = "Missing or expired session.";
    public const string UserBusy = "User is busy, try again.";
    public const string AccountDisabled = "This account is disabled.";
    public const string AccountBanned = "This account is banned.";
    public const string RegistrationClosed = "New accounts are by invite only: use Data Transfer with your transfer code.";
    public const string NoLinkedTransfer = "No account was linked: enter your transfer code on the Data Transfer page first.";

    public static string NoSaveToTransfer(string where)
        => $"No save to transfer: no EntityIUserTable.json with a user ({where})";
}
