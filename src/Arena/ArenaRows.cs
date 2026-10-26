namespace MariesWonderland.Arena;

/// <summary>
/// One player's (or bot's) place in one Arena season: points, record, the defense deck others see on their cards,
/// and the opponent list on offer. Players get one when they first open the Arena in a season; the season's bots
/// are made when its first player arrives and removed once it has ended and been settled.
/// </summary>
public sealed class ArenaEntry
{
    public int SeasonId { get; set; }

    /// <summary>The player's user id; for a bot the same as its <see cref="PlayerId"/>.</summary>
    public long UserId { get; set; }

    public long PlayerId { get; set; }
    public bool IsBot { get; set; }
    public long SeasonEndsAt { get; set; }

    public int Points { get; set; }

    /// <summary>When <see cref="Points"/> last changed: on equal points, whoever got there first ranks higher.</summary>
    public long PointsChangedAt { get; set; }

    public int AttackWins { get; set; }
    public int AttackLosses { get; set; }
    public int AttackPointsChange { get; set; }
    public int DefenseWins { get; set; }
    public int DefenseLosses { get; set; }
    public int DefensePointsChange { get; set; }

    /// <summary>The defense deck as opponent cards and the ranking show it, taken when the player last used the Arena or changed the deck.</summary>
    public int DefenseDeckPower { get; set; }
    public List<int> DefenseCostumeIds { get; set; } = [];
    public List<int> DefenseAttributeTypes { get; set; } = [];
    public int MostPowerfulCostumeId { get; set; }

    /// <summary>The opponents offered now (user ids), in card order.</summary>
    public List<long> MatchingUserIds { get; set; } = [];

    /// <summary>Bots only: the name shown and the seed their deck is built from.</summary>
    public string BotName { get; set; } = "";
    public int BotSeed { get; set; }
    public int BotFavoriteCostumeId { get; set; }

    /// <summary>The player's place when the season ended; 0 while it runs.</summary>
    public int SeasonFinalRank { get; set; }
}

/// <summary>An Arena battle. Written when it starts (<see cref="FinishedAt"/> 0) and completed when it ends.</summary>
public sealed class ArenaBattle
{
    public long BattleId { get; set; }
    public int SeasonId { get; set; }
    public long AttackerUserId { get; set; }
    public long DefenderUserId { get; set; }
    public long StartedAt { get; set; }
    public long FinishedAt { get; set; }
    public bool AttackerWon { get; set; }
    public int AttackerPointsChange { get; set; }
    public int DefenderPointsChange { get; set; }

    /// <summary>Both decks as the battle logs show them, taken at the start.</summary>
    public int AttackerDeckPower { get; set; }
    public List<int> AttackerCostumeIds { get; set; } = [];
    public int DefenderDeckPower { get; set; }
    public List<int> DefenderCostumeIds { get; set; } = [];
}

/// <summary>
/// A player's result for a week they attacked in (the week starts Monday 08:00 UTC). Points and place are kept up to
/// date while the week runs and frozen when it ends; the player's <c>EntityIUserPvpWeeklyResult</c> rows copy these.
/// </summary>
public sealed class ArenaWeek
{
    public long UserId { get; set; }
    public long WeekStart { get; set; }
    public int SeasonId { get; set; }
    public int FinalPoint { get; set; }
    public int FinalRank { get; set; }
    public bool Closed { get; set; }
}

/// <summary>Everything saved about the Arena, as loaded at startup.</summary>
public sealed record ArenaState(IReadOnlyList<ArenaEntry> Entries, IReadOnlyList<ArenaBattle> Battles, IReadOnlyList<ArenaWeek> Weeks);

/// <summary>The rows one Arena operation added or changed, and the seasons it cleared, written in one transaction.</summary>
public sealed class ArenaChanges
{
    public List<ArenaEntry> SavedEntries { get; } = [];
    public List<ArenaBattle> SavedBattles { get; } = [];
    public List<long> RemovedBattleIds { get; } = [];
    public List<ArenaWeek> SavedWeeks { get; } = [];

    /// <summary>Seasons that ended and were settled: their bots and battles are deleted (players' entries stay).</summary>
    public List<int> ClearedSeasons { get; } = [];

    public bool IsEmpty => SavedEntries.Count == 0 && SavedBattles.Count == 0 && RemovedBattleIds.Count == 0
        && SavedWeeks.Count == 0 && ClearedSeasons.Count == 0;
}
