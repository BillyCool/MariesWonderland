namespace MariesWonderland.Models.Type;

/// <summary>
/// The play-history counters (<c>GetUserGamePlayNote</c>'s type id, the profile's history items). The client has no
/// enum for them; the names come from the <c>profile.history.name.NNN</c> text.
/// </summary>
public enum GamePlayHistoryType
{
    UNKNOWN = 0,
    CHAPTER_SUMMONS_PERFORMED = 1,
    EVENT_SUMMONS_PERFORMED = 2,
    ITEM_SHOP_PURCHASES = 3,
    EXCHANGE_TRADES = 4,
    PARTY_ELIMINATED = 5,
    SHOOTING_GAMES_PLAYED = 6,
    EXPLORATION_2_GAMES_PLAYED = 7,
    EXPLORATION_3_GAMES_PLAYED = 8,
    EXPLORATION_4_GAMES_PLAYED = 9,
    PICTUREBOOKS_WATCHED = 10,
    BEST_ARENA_WIN_STREAK = 11,
    MISSIONS_COMPLETED = 12,
    /// <summary>Metres walked in The Cage. The smartphone's pedometer asks for this one.</summary>
    CAGE_WALKED_DISTANCE = 13,
    CHARACTER_TYPES_COLLECTED = 14,
    WEAPON_TYPES_COLLECTED = 15,
    COMPANION_TYPES_COLLECTED = 16,
    MEMOIR_TYPES_COLLECTED = 17,
    DAYS_LOGGED_IN = 18,
    ARENA_MATCHES_PLAYED = 19,
    CHARACTER_ENHANCEMENTS = 20,
    CHARACTER_SKILL_ENHANCEMENTS = 21,
    CHARACTER_ASCENSIONS = 22,
    WEAPON_ENHANCEMENTS = 23,
    WEAPON_SKILL_ENHANCEMENTS = 24,
    WEAPON_EVOLUTIONS = 25,
    WEAPON_ASCENSIONS = 26,
    COMPANION_ENHANCEMENTS = 27,
    MEMOIR_ENHANCEMENTS = 28,
    BLACK_BIRDS_SLAIN = 29
}
