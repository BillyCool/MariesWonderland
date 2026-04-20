namespace MariesWonderland;

public static class Constants
{
    public const long MinPlayerId = 1_000_000_000_000L; // 1e12, 1 trillion

    public const long MaxPlayerId = 2_000_000_000_000L; // 2e12, 2 trillion

    public const long MinUserId = 1_000_000_000_000_000_000L; // 1e18, 1 quintillion

    public const long MaxUserId = 2_000_000_000_000_000_000L; // 2e18, 2 quintillion

    public static readonly List<int> StartingWeaponIds = [100001, 100011, 100021]; // Deathpierce Sword, Deathshot Pistol, Deathstrike Staff, 

    public const int StartingDeckCostumeId = 10100; // Rion

    public const int StartingDeckWeaponId = 101001; // Everlasting Cardia
}
