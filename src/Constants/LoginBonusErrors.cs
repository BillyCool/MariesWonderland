namespace MariesWonderland.Constants;

public static class LoginBonusErrors
{
    public static string Exhausted(int loginBonusId, int page, int stamp)
        => $"login bonus {loginBonusId} exhausted at page {page} stamp {stamp}";

    public static string RewardNotFound(int loginBonusId, int page, int stamp)
        => $"no reward found for login bonus {loginBonusId} page {page} stamp {stamp}";
}
