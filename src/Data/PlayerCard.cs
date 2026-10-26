using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Data;

/// <summary>
/// What other players see of a player in lists (friends, requests, search): name, level, icon, best quest deck
/// power ("Total Force") and last login. Read without loading or locking the player.
/// </summary>
public sealed record PlayerCard(long UserId, long PlayerId, string Name, int Level, int FavoriteCostumeId, int MaxDeckPower, long LastLoginDatetime)
{
    /// <summary>The card of a user in memory; null when they have no user row.</summary>
    public static PlayerCard? From(DarkUserMemoryDatabase userDb)
    {
        EntityIUser? user = userDb.EntityIUser.FirstOrDefault();
        if (user == null)
        {
            return null;
        }

        EntityIUserProfile? profile = userDb.EntityIUserProfile.FirstOrDefault();
        return new PlayerCard(
            user.UserId,
            user.PlayerId,
            profile?.Name ?? string.Empty,
            userDb.EntityIUserStatus.FirstOrDefault()?.Level ?? 0,
            profile?.FavoriteCostumeId ?? 0,
            userDb.EntityIUserDeckTypeNote.FirstOrDefault(n => n.DeckType == DeckType.QUEST)?.MaxDeckPower ?? 0,
            userDb.EntityIUserLogin.FirstOrDefault()?.LastLoginDatetime ?? 0);
    }
}
