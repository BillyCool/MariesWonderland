using MariesWonderland.Data;

namespace MariesWonderland.Helpers;

/// <summary>Which gear is worn by a deck: it can't be sold or used up while it is (the client doesn't offer it, and the server refuses too).</summary>
public static class DeckGearHelper
{
    /// <summary>Whether a deck has the weapon as a main or a sub weapon.</summary>
    public static bool IsWeaponInDeck(DarkUserMemoryDatabase userDb, string userWeaponUuid)
        => userDb.EntityIUserDeckCharacter.Any(c => c.MainUserWeaponUuid == userWeaponUuid)
            || userDb.EntityIUserDeckSubWeaponGroup.Any(s => s.UserWeaponUuid == userWeaponUuid);

    /// <summary>Whether a deck has the memoir.</summary>
    public static bool IsPartsInDeck(DarkUserMemoryDatabase userDb, string userPartsUuid)
        => userDb.EntityIUserDeckPartsGroup.Any(p => p.UserPartsUuid == userPartsUuid);
}
