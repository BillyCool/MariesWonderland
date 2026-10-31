using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.User;

namespace MariesWonderland.Helpers;

/// <summary>The server-kept play-history counters (<see cref="EntitySPlayHistory"/>) and what the profile shows of them.</summary>
public static class PlayHistoryHelper
{
    /// <summary>
    /// The profile's history list: the client has eight slots and fills them in the order sent. These are the ones
    /// (and the order) the original profile showed.
    /// TODO: the original rotated which of the 29 stats were shown; for now these eight are always sent. The rule is
    /// unknown (see the smartphone spec).
    /// </summary>
    private static readonly GamePlayHistoryType[] ProfileItems =
    [
        GamePlayHistoryType.CHAPTER_SUMMONS_PERFORMED,
        GamePlayHistoryType.SHOOTING_GAMES_PLAYED,
        GamePlayHistoryType.BLACK_BIRDS_SLAIN,
        GamePlayHistoryType.WEAPON_TYPES_COLLECTED,
        GamePlayHistoryType.MEMOIR_ENHANCEMENTS,
        GamePlayHistoryType.CHARACTER_ENHANCEMENTS,
        GamePlayHistoryType.PARTY_ELIMINATED,
        GamePlayHistoryType.DAYS_LOGGED_IN,
    ];

    /// <summary>
    /// Which counters belong to each radar axis, by <c>categoryTypeId</c> (the order of
    /// <c>ui.Outgame.Profile.Graph.ParamN</c>: Greed, Vice, Mania, Zeal, Will, Wisdom). The player community's
    /// grouping; every counter sits on one axis. Not used yet: see the TODO on <see cref="ForProfile"/>.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, GamePlayHistoryType[]> GraphAxes = new Dictionary<int, GamePlayHistoryType[]>
    {
        // Greed
        [1] = [GamePlayHistoryType.CHAPTER_SUMMONS_PERFORMED, GamePlayHistoryType.EVENT_SUMMONS_PERFORMED, GamePlayHistoryType.ITEM_SHOP_PURCHASES, GamePlayHistoryType.EXCHANGE_TRADES],
        // Vice
        [2] = [GamePlayHistoryType.SHOOTING_GAMES_PLAYED, GamePlayHistoryType.EXPLORATION_2_GAMES_PLAYED, GamePlayHistoryType.EXPLORATION_3_GAMES_PLAYED, GamePlayHistoryType.EXPLORATION_4_GAMES_PLAYED],
        // Mania
        [3] = [GamePlayHistoryType.CHARACTER_TYPES_COLLECTED, GamePlayHistoryType.WEAPON_TYPES_COLLECTED, GamePlayHistoryType.COMPANION_TYPES_COLLECTED, GamePlayHistoryType.MEMOIR_TYPES_COLLECTED],
        // Zeal
        [4] =
        [
            GamePlayHistoryType.CHARACTER_ENHANCEMENTS, GamePlayHistoryType.CHARACTER_SKILL_ENHANCEMENTS, GamePlayHistoryType.CHARACTER_ASCENSIONS,
            GamePlayHistoryType.WEAPON_ENHANCEMENTS, GamePlayHistoryType.WEAPON_SKILL_ENHANCEMENTS, GamePlayHistoryType.WEAPON_EVOLUTIONS,
            GamePlayHistoryType.WEAPON_ASCENSIONS, GamePlayHistoryType.COMPANION_ENHANCEMENTS, GamePlayHistoryType.MEMOIR_ENHANCEMENTS,
        ],
        // Will
        [5] = [GamePlayHistoryType.PARTY_ELIMINATED, GamePlayHistoryType.BEST_ARENA_WIN_STREAK, GamePlayHistoryType.DAYS_LOGGED_IN, GamePlayHistoryType.ARENA_MATCHES_PLAYED, GamePlayHistoryType.BLACK_BIRDS_SLAIN],
        // Wisdom
        [6] = [GamePlayHistoryType.PICTUREBOOKS_WATCHED, GamePlayHistoryType.MISSIONS_COMPLETED, GamePlayHistoryType.CAGE_WALKED_DISTANCE],
    };

    public static void Add(DarkUserMemoryDatabase userDb, GamePlayHistoryType type, long amount)
    {
        EntitySPlayHistory row = userDb.EntitySPlayHistory.GetOrCreate(
            h => h.GamePlayHistoryType == type,
            () => new EntitySPlayHistory { UserId = userDb.UserId, GamePlayHistoryType = type });
        row.Count += amount;
    }

    /// <summary>Raises a "best ever" counter (a win streak) to <paramref name="value"/> when it beats the record.</summary>
    public static void Best(DarkUserMemoryDatabase userDb, GamePlayHistoryType type, long value)
    {
        EntitySPlayHistory row = userDb.EntitySPlayHistory.GetOrCreate(
            h => h.GamePlayHistoryType == type,
            () => new EntitySPlayHistory { UserId = userDb.UserId, GamePlayHistoryType = type });
        row.Count = Math.Max(row.Count, value);
    }

    /// <summary>
    /// The counter's value; 0 when nothing was recorded. Days logged in and the "types collected" counters are
    /// read off the account, so they are right for an imported save too.
    /// </summary>
    public static long Get(DarkUserMemoryDatabase userDb, GamePlayHistoryType type) => type switch
    {
        GamePlayHistoryType.DAYS_LOGGED_IN => userDb.EntityIUserLogin.FirstOrDefault()?.TotalLoginCount ?? 0,
        GamePlayHistoryType.CHARACTER_TYPES_COLLECTED => userDb.EntityIUserCharacter.Count,
        GamePlayHistoryType.WEAPON_TYPES_COLLECTED => userDb.EntityIUserWeaponNote.Count,
        GamePlayHistoryType.COMPANION_TYPES_COLLECTED => userDb.EntityIUserCompanion.Select(c => c.CompanionId).Distinct().Count(),
        GamePlayHistoryType.MEMOIR_TYPES_COLLECTED => userDb.EntityIUserPartsGroupNote.Count,
        _ => userDb.EntitySPlayHistory.FirstOrDefault(h => h.GamePlayHistoryType == type)?.Count ?? 0,
    };

    /// <summary>
    /// What a profile shows: the history list and the radar chart. The radar's six axes are sent empty.
    /// TODO: fill the axes from <see cref="GraphAxes"/> (see the smartphone spec for the open design: a recent
    /// window or all time, caps).
    /// </summary>
    public static GamePlayHistory ForProfile(DarkUserMemoryDatabase userDb)
    {
        GamePlayHistory history = new();
        foreach (GamePlayHistoryType type in ProfileItems)
        {
            history.HistoryItem.Add(new PlayHistoryItem { HistoryItemId = (int)type, Count = Get(userDb, type) });
        }

        foreach (int categoryTypeId in GraphAxes.Keys)
        {
            history.HistoryCategoryGraphItem.Add(new PlayHistoryCategoryGraphItem { CategoryTypeId = categoryTypeId, ProgressPermil = 0 });
        }

        return history;
    }
}
