namespace MariesWonderland.Constants;

public static class GameConstants
{
    public const long MinPlayerId = 100_000_000_000L; // 1e11, the smallest 12-digit number

    public const long MaxPlayerId = 1_000_000_000_000L; // 1e12, the first 13-digit number (exclusive)

    public const long MinUserId = 1_000_000_000_000_000_000L; // 1e18, 1 quintillion

    public const long MaxUserId = 2_000_000_000_000_000_000L; // 2e18, 2 quintillion

    public const int StartingDeckCostumeId = 10100; // Rion

    public const int StartingDeckWeaponId = 101001; // Everlasting Cardia
}
