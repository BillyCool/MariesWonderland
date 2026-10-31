namespace MariesWonderland.Constants;

public static class SystemErrors
{
    public const string MasterDataOutOfDate = "Master data is out of date";
    public const string MissingSession = "Missing or expired session.";
    public const string UserBusy = "User is busy, try again.";

    public static string NoSaveToTransfer(string where)
        => $"No save to transfer: no EntityIUserTable.json with a user ({where})";
}
