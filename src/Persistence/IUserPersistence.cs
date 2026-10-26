using MariesWonderland.Data;
using MariesWonderland.Friends;
using MariesWonderland.Models.Entities;

namespace MariesWonderland.Persistence;

/// <summary>
/// Where <see cref="UserDataStore"/> keeps users between restarts. The store works on in-memory user databases and
/// writes each request's changed rows here; users that aren't in memory are loaded from here.
/// </summary>
public interface IUserPersistence
{
    /// <summary>The user's saved rows, or null when the user was never saved.</summary>
    DarkUserMemoryDatabase? LoadUser(long userId);

    /// <summary>Writes the rows a request added, changed or removed, in one transaction.</summary>
    void SaveChanges(long userId, UserChangeSet changes);

    /// <summary>Replaces everything saved for the user with <paramref name="userDb"/> as it is now.</summary>
    void ReplaceUser(DarkUserMemoryDatabase userDb);

    /// <summary>Appends a finished battle to the battle log.</summary>
    void AddBattleDetail(EntitySBattleDetail detail);

    /// <summary>The user's battle log, oldest first.</summary>
    IReadOnlyList<EntitySBattleDetail> BattleDetails(long userId);

    /// <summary>The battles of one quest recorded at or after <paramref name="sinceUtc"/>, oldest first.</summary>
    IReadOnlyList<EntitySBattleDetail> BattleDetails(long userId, int questId, DateTime sinceUtc);

    /// <summary>The ids of every saved user.</summary>
    IReadOnlyList<long> UserIds();

    /// <summary>The user a player id belongs to, if any.</summary>
    long? FindUserIdByPlayerId(long playerId);

    /// <summary>The user a device id signs in as, if any.</summary>
    long? FindUserIdByDevice(string uuid);

    void SaveDevice(string uuid, long userId);

    /// <summary>Every device id and its user.</summary>
    IReadOnlyDictionary<string, long> Devices();

    UserSession? FindSession(string sessionKey);

    /// <summary>Saves a new session and drops the ones expired by <paramref name="now"/>.</summary>
    void SaveSession(UserSession session, DateTime now);

    IReadOnlyList<UserSession> Sessions();

    IReadOnlyDictionary<string, int> SharedPrizeClaims();

    void SaveSharedPrizeClaim(string prizeKey, int claimed);

    /// <summary>Every friendship, friend request and cheer.</summary>
    FriendState LoadFriends();

    /// <summary>Writes what one friend operation changed, in one transaction (removals first).</summary>
    void SaveFriendChanges(FriendChanges changes);

    /// <summary>The cards of these users (all users when null); users without a user row are left out.</summary>
    IReadOnlyList<PlayerCard> PlayerCards(IReadOnlyCollection<long>? userIds);
}
