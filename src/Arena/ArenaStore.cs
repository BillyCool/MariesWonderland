using MariesWonderland.Helpers;
using MariesWonderland.Persistence;

namespace MariesWonderland.Arena;

/// <summary>An Arena season as the store needs it (from master data).</summary>
public sealed record ArenaSeason(int SeasonId, long StartsAt, long EndsAt);

/// <summary>A player's defense deck as opponent cards and the ranking show it.</summary>
public sealed record ArenaDefense(int DeckPower, IReadOnlyList<int> CostumeIds, IReadOnlyList<int> AttributeTypes, int MostPowerfulCostumeId)
{
    public static readonly ArenaDefense None = new(0, [], [], 0);
}

/// <summary>A copy of one entry with its place in the season's ranking (1 = first).</summary>
public sealed record ArenaStanding(
    long UserId, long PlayerId, bool IsBot, string BotName, int BotSeed, int BotFavoriteCostumeId,
    int Points, int Rank, ArenaDefense Defense,
    int AttackWins, int AttackLosses, int AttackPointsChange, int DefenseWins, int DefenseLosses, int DefensePointsChange,
    int SeasonFinalRank);

/// <summary>What a finished battle did to the attacker.</summary>
public sealed record ArenaFinish(int BeforePoints, int BeforeRank, int AfterPoints, int AfterRank, int PointsChange);

/// <summary>One line of a battle history, seen from the player asking: the other side, as they are now and as their deck was.</summary>
public sealed record ArenaLogLine(ArenaStanding Opponent, bool IsVictory, int PointsChange, long BattleAt, int OpponentDeckPower, IReadOnlyList<int> OpponentCostumeIds);

/// <summary>Why an Arena operation was refused, or <see cref="Ok"/>.</summary>
public enum ArenaOutcome
{
    Ok,
    NotEntered,
    NotInMatchingList,
    NoBattleInProgress,
}

/// <summary>
/// Arena standings, battles and weekly results. They belong to two players at once (attacker and defender) and to
/// bots, so they live here, server-wide, rather than in any user's data; nothing here locks a user. Kept in memory and
/// written through to the user database (when there is one), each operation under one lock.
/// Every operation first settles what time has finished: weeks that ended get their final points and places, and
/// seasons that ended get each player's final place, after which their bots and battles are dropped. Nothing changes
/// points before that happens, so a week's final points are exactly the points when it ended.
/// </summary>
public sealed class ArenaStore
{
    public const int RankingPageSize = 100;
    public const int MatchingCount = 3;
    public const int LogLength = 50;

    /// <summary>
    /// Opponents are drawn from players and bots within this many points of the player.
    /// TODO: review. The owner remembers it as roughly 5,000 but isn't sure; neither the client nor master data says.
    /// </summary>
    public const int MatchingRange = 5_000;

    private static readonly long WeekMs = (long)TimeSpan.FromDays(7).TotalMilliseconds;

    private readonly Lock _lock = new();
    private readonly IUserPersistence? _persistence;
    private readonly Dictionary<(int SeasonId, long UserId), ArenaEntry> _entries = [];
    private readonly Dictionary<long, ArenaBattle> _battles = [];
    private readonly Dictionary<(long UserId, long WeekStart), ArenaWeek> _weeks = [];
    private long _lastBattleId;

    public ArenaStore(IUserPersistence? persistence = null)
    {
        _persistence = persistence;
        ArenaState? state = persistence?.LoadArena();
        if (state == null)
        {
            return;
        }

        foreach (ArenaEntry entry in state.Entries)
        {
            _entries[(entry.SeasonId, entry.UserId)] = entry;
        }
        foreach (ArenaBattle battle in state.Battles)
        {
            _battles[battle.BattleId] = battle;
            _lastBattleId = Math.Max(_lastBattleId, battle.BattleId);
        }
        foreach (ArenaWeek week in state.Weeks)
        {
            _weeks[(week.UserId, week.WeekStart)] = week;
        }
    }

    // ---- Entering a season ----

    /// <summary>
    /// The player's standing in <paramref name="season"/>, entering them first with <paramref name="initialPoints"/>
    /// when they have none. The season's bots are made (by <paramref name="createBots"/>) when its first player
    /// arrives. <paramref name="defense"/>, when given, refreshes what others see of their defense deck.
    /// </summary>
    public ArenaStanding Enter(ArenaSeason season, long userId, long playerId, int initialPoints, ArenaDefense? defense, long nowMs, Func<IEnumerable<ArenaEntry>> createBots, out bool isNew)
    {
        bool entered = false;
        ArenaStanding standing = Write(nowMs, changes =>
        {
            if (!_entries.Values.Any(e => e.SeasonId == season.SeasonId && e.IsBot))
            {
                foreach (ArenaEntry bot in createBots())
                {
                    bot.SeasonId = season.SeasonId;
                    bot.IsBot = true;
                    bot.SeasonEndsAt = season.EndsAt;
                    _entries[(season.SeasonId, bot.UserId)] = bot;
                    changes.SavedEntries.Add(bot);
                }
            }

            if (!_entries.TryGetValue((season.SeasonId, userId), out ArenaEntry? entry))
            {
                entry = new ArenaEntry
                {
                    SeasonId = season.SeasonId,
                    UserId = userId,
                    PlayerId = playerId,
                    SeasonEndsAt = season.EndsAt,
                    Points = initialPoints,
                    PointsChangedAt = nowMs,
                };
                _entries[(season.SeasonId, userId)] = entry;
                entered = true;
                changes.SavedEntries.Add(entry);
            }

            if (defense != null && SetDefense(entry, defense))
            {
                changes.SavedEntries.Add(entry);
            }
            return StandingOf(entry, Ranks(season.SeasonId));
        });
        isNew = entered;
        return standing;
    }

    /// <summary>Refreshes what others see of the player's defense deck in a season they're in; no-op otherwise.</summary>
    public void UpdateDefense(int seasonId, long userId, ArenaDefense defense, long nowMs)
        => Write(nowMs, changes =>
        {
            if (_entries.TryGetValue((seasonId, userId), out ArenaEntry? entry) && SetDefense(entry, defense))
            {
                changes.SavedEntries.Add(entry);
            }
            return 0;
        });

    // ---- Reads ----

    /// <summary>The player's (or bot's) standing in a season; null when they aren't in it.</summary>
    public ArenaStanding? Standing(int seasonId, long userId, long nowMs)
        => Write(nowMs, _ => _entries.TryGetValue((seasonId, userId), out ArenaEntry? entry) ? StandingOf(entry, Ranks(seasonId)) : null);

    /// <summary>A player or bot of the season by player id; null when there's none.</summary>
    public ArenaStanding? FindByPlayerId(int seasonId, long playerId, long nowMs)
        => Write(nowMs, _ =>
        {
            ArenaEntry? entry = _entries.Values.FirstOrDefault(e => e.SeasonId == seasonId && e.PlayerId == playerId);
            return entry == null ? null : StandingOf(entry, Ranks(seasonId));
        });

    /// <summary>A bot of any season still kept, by player id (bots of ended seasons are dropped when it's settled).</summary>
    public ArenaStanding? FindBot(long playerId, long nowMs)
        => Write(nowMs, _ =>
        {
            ArenaEntry? bot = _entries.Values.FirstOrDefault(e => e.IsBot && e.PlayerId == playerId);
            return bot == null ? null : StandingOf(bot, Ranks(bot.SeasonId));
        });

    /// <summary>A page of the ranking from <paramref name="rankFrom"/> (1-based), and how many are ranked.</summary>
    public (IReadOnlyList<ArenaStanding> Page, int Count) Ranking(int seasonId, int rankFrom, int count, long nowMs)
        => Write(nowMs, _ =>
        {
            List<ArenaEntry> ranked = Ranked(seasonId);
            int skip = Math.Max(rankFrom, 1) - 1;
            IReadOnlyList<ArenaStanding> page = [.. ranked.Skip(skip).Take(count).Select((e, i) => StandingOf(e, skip + i + 1))];
            return (page, ranked.Count);
        });

    /// <summary>The opponents on offer now, in card order (ones no longer in the season are left out).</summary>
    public IReadOnlyList<ArenaStanding> Matching(int seasonId, long userId, long nowMs)
        => Write(nowMs, _ =>
        {
            if (!_entries.TryGetValue((seasonId, userId), out ArenaEntry? entry))
            {
                return [];
            }

            Dictionary<long, int> ranks = Ranks(seasonId);
            return (IReadOnlyList<ArenaStanding>)[.. entry.MatchingUserIds
                .Select(id => _entries.GetValueOrDefault((seasonId, id)))
                .OfType<ArenaEntry>()
                .Select(e => StandingOf(e, ranks))];
        });

    /// <summary>The player's attacks (or the attacks on them) this season, newest first.</summary>
    public IReadOnlyList<ArenaLogLine> Log(int seasonId, long userId, bool asAttacker, long nowMs)
        => Write(nowMs, _ =>
        {
            Dictionary<long, int> ranks = Ranks(seasonId);
            List<ArenaLogLine> lines = [];
            foreach (ArenaBattle battle in _battles.Values
                .Where(b => b.SeasonId == seasonId && b.FinishedAt != 0 && (asAttacker ? b.AttackerUserId : b.DefenderUserId) == userId)
                .OrderByDescending(b => b.FinishedAt)
                .Take(LogLength))
            {
                long opponentId = asAttacker ? battle.DefenderUserId : battle.AttackerUserId;
                if (!_entries.TryGetValue((seasonId, opponentId), out ArenaEntry? opponent))
                {
                    continue;
                }

                lines.Add(asAttacker
                    ? new ArenaLogLine(StandingOf(opponent, ranks), battle.AttackerWon, battle.AttackerPointsChange, battle.FinishedAt, battle.DefenderDeckPower, battle.DefenderCostumeIds)
                    : new ArenaLogLine(StandingOf(opponent, ranks), !battle.AttackerWon, battle.DefenderPointsChange, battle.FinishedAt, battle.AttackerDeckPower, battle.AttackerCostumeIds));
            }
            return (IReadOnlyList<ArenaLogLine>)lines;
        });

    /// <summary>The player's weekly results, oldest first (open weeks hold their current points and place).</summary>
    public IReadOnlyList<ArenaWeek> Weeks(long userId, long nowMs)
        => Write(nowMs, _ => (IReadOnlyList<ArenaWeek>)[.. _weeks.Values
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.WeekStart)
            .Select(w => new ArenaWeek { UserId = w.UserId, WeekStart = w.WeekStart, SeasonId = w.SeasonId, FinalPoint = w.FinalPoint, FinalRank = w.FinalRank, Closed = w.Closed })]);

    /// <summary>The player's most recently started Arena battle (finished or not): its id and start time; null when none is kept.</summary>
    public (long BattleId, long StartedAt)? LatestBattle(long userId, long nowMs)
        => Write(nowMs, _ =>
        {
            ArenaBattle? battle = _battles.Values.Where(b => b.AttackerUserId == userId).MaxBy(b => b.StartedAt);
            return battle == null ? ((long, long)?)null : (battle.BattleId, battle.StartedAt);
        });

    // ---- Opponents and battles ----

    /// <summary>
    /// Draws a new opponent list: <see cref="MatchingCount"/> at random from the players and bots within
    /// <see cref="MatchingRange"/> points of the player (players only with a defense deck), highest points first. When
    /// fewer than that are in range, the nearest ones outside it fill the list.
    /// </summary>
    public IReadOnlyList<ArenaStanding> NewMatching(int seasonId, long userId, Random random, long nowMs)
        => Write(nowMs, changes =>
        {
            if (!_entries.TryGetValue((seasonId, userId), out ArenaEntry? entry))
            {
                return [];
            }

            List<(ArenaEntry Entry, long Distance)> candidates = [.. _entries.Values
                .Where(e => e.SeasonId == seasonId && e.UserId != userId && (e.IsBot || e.DefenseCostumeIds.Count > 0))
                .Select(e => (e, Math.Abs((long)e.Points - entry.Points)))];
            List<ArenaEntry> picked = [.. candidates
                .Where(c => c.Distance <= MatchingRange)
                .Select(c => c.Entry)
                .OrderBy(_ => random.Next())
                .Take(MatchingCount)];
            if (picked.Count < MatchingCount)
            {
                picked.AddRange(candidates
                    .Where(c => c.Distance > MatchingRange)
                    .OrderBy(c => c.Distance)
                    .ThenBy(_ => random.Next())
                    .Take(MatchingCount - picked.Count)
                    .Select(c => c.Entry));
            }
            picked = [.. picked.OrderByDescending(e => e.Points)];

            entry.MatchingUserIds = [.. picked.Select(e => e.UserId)];
            changes.SavedEntries.Add(entry);
            Dictionary<long, int> ranks = Ranks(seasonId);
            return (IReadOnlyList<ArenaStanding>)[.. picked.Select(e => StandingOf(e, ranks))];
        });

    /// <summary>Whether the player may attack this opponent now: they're in the season and on the player's list.</summary>
    public ArenaOutcome CanStart(int seasonId, long userId, long opponentUserId, long nowMs)
        => Write(nowMs, _ => CanStartLocked(seasonId, userId, opponentUserId));

    /// <summary>Records a battle starting, with both decks as the logs will show them. A battle left unfinished is dropped.</summary>
    public ArenaOutcome StartBattle(int seasonId, long userId, long opponentUserId, ArenaDefense attackerDeck, ArenaDefense defenderDeck, long nowMs)
        => Write(nowMs, changes =>
        {
            ArenaOutcome outcome = CanStartLocked(seasonId, userId, opponentUserId);
            if (outcome != ArenaOutcome.Ok)
            {
                return outcome;
            }

            foreach (ArenaBattle stale in _battles.Values.Where(b => b.AttackerUserId == userId && b.FinishedAt == 0).ToList())
            {
                _battles.Remove(stale.BattleId);
                changes.RemovedBattleIds.Add(stale.BattleId);
            }

            ArenaBattle battle = new()
            {
                BattleId = ++_lastBattleId,
                SeasonId = seasonId,
                AttackerUserId = userId,
                DefenderUserId = opponentUserId,
                StartedAt = nowMs,
                AttackerDeckPower = attackerDeck.DeckPower,
                AttackerCostumeIds = [.. attackerDeck.CostumeIds],
                DefenderDeckPower = defenderDeck.DeckPower,
                DefenderCostumeIds = [.. defenderDeck.CostumeIds],
            };
            _battles[battle.BattleId] = battle;
            changes.SavedBattles.Add(battle);
            return ArenaOutcome.Ok;
        });

    /// <summary>
    /// Ends the player's battle against <paramref name="opponentUserId"/>: moves both sides' points (bots too), counts
    /// the result and updates this week's result. Null when there's no such battle in progress.
    /// </summary>
    public ArenaFinish? FinishBattle(int seasonId, long userId, long opponentUserId, bool isVictory, long nowMs)
        => Write(nowMs, changes =>
        {
            ArenaBattle? battle = _battles.Values.FirstOrDefault(b =>
                b.SeasonId == seasonId && b.AttackerUserId == userId && b.DefenderUserId == opponentUserId && b.FinishedAt == 0);
            if (battle == null
                || !_entries.TryGetValue((seasonId, userId), out ArenaEntry? attacker)
                || !_entries.TryGetValue((seasonId, opponentUserId), out ArenaEntry? defender))
            {
                return null;
            }

            int beforePoints = attacker.Points;
            int beforeRank = Ranks(seasonId)[userId];
            int change = isVictory
                ? ArenaPoints.WinGain(attacker.Points, defender.Points)
                : -ArenaPoints.LossCost(attacker.Points, defender.Points);
            int attackerChange = Move(attacker, change, nowMs);
            int defenderChange = Move(defender, -change, nowMs);

            if (isVictory)
            {
                attacker.AttackWins++;
                defender.DefenseLosses++;
            }
            else
            {
                attacker.AttackLosses++;
                defender.DefenseWins++;
            }
            attacker.AttackPointsChange += attackerChange;
            defender.DefensePointsChange += defenderChange;

            battle.FinishedAt = nowMs;
            battle.AttackerWon = isVictory;
            battle.AttackerPointsChange = attackerChange;
            battle.DefenderPointsChange = defenderChange;

            int afterRank = Ranks(seasonId)[userId];
            long weekStart = GameDayHelper.StartOfWeek(nowMs);
            if (!_weeks.TryGetValue((userId, weekStart), out ArenaWeek? week))
            {
                week = new ArenaWeek { UserId = userId, WeekStart = weekStart, SeasonId = seasonId };
                _weeks[(userId, weekStart)] = week;
            }
            week.FinalPoint = attacker.Points;
            week.FinalRank = afterRank;

            changes.SavedEntries.Add(attacker);
            changes.SavedEntries.Add(defender);
            changes.SavedBattles.Add(battle);
            changes.SavedWeeks.Add(week);
            return new ArenaFinish(beforePoints, beforeRank, attacker.Points, afterRank, attackerChange);
        });

    // ---- Internals (callers hold the lock) ----

    /// <summary>Runs one change under the lock, after settling what has ended, and saves what it changed.</summary>
    private T Write<T>(long nowMs, Func<ArenaChanges, T> change)
    {
        lock (_lock)
        {
            ArenaChanges changes = new();
            Settle(nowMs, changes);
            T result = change(changes);
            if (!changes.IsEmpty)
            {
                _persistence?.SaveArenaChanges(changes);
            }
            return result;
        }
    }

    /// <summary>
    /// Freezes weeks that ended and settles seasons that ended, in time order. A week whose season ended first closes
    /// with the season (its bots are still there for the ranking); at the same instant weeks go before seasons.
    /// </summary>
    private void Settle(long nowMs, ArenaChanges changes)
    {
        Dictionary<int, long> seasonEnds = [];
        foreach (ArenaEntry entry in _entries.Values)
        {
            if (entry.SeasonEndsAt <= nowMs && (entry.IsBot || entry.SeasonFinalRank == 0))
            {
                seasonEnds[entry.SeasonId] = entry.SeasonEndsAt;
            }
        }

        List<(long At, int Kind, int SeasonId, long WeekStart)> events = [];
        foreach (IGrouping<(int SeasonId, long WeekStart), ArenaWeek> week in _weeks.Values.Where(w => !w.Closed).GroupBy(w => (w.SeasonId, w.WeekStart)))
        {
            long endsAt = week.Key.WeekStart + WeekMs;
            long seasonEnd = _entries.Values.FirstOrDefault(e => e.SeasonId == week.Key.SeasonId)?.SeasonEndsAt ?? endsAt;
            long closesAt = Math.Min(endsAt, seasonEnd);
            if (closesAt <= nowMs)
            {
                events.Add((closesAt, 0, week.Key.SeasonId, week.Key.WeekStart));
            }
        }
        foreach ((int seasonId, long endsAt) in seasonEnds)
        {
            events.Add((endsAt, 1, seasonId, 0));
        }

        foreach ((long _, int kind, int seasonId, long weekStart) in events.OrderBy(e => e.At).ThenBy(e => e.Kind))
        {
            Dictionary<long, int> ranks = Ranks(seasonId);
            if (kind == 0)
            {
                foreach (ArenaWeek week in _weeks.Values.Where(w => w.SeasonId == seasonId && w.WeekStart == weekStart && !w.Closed))
                {
                    if (_entries.TryGetValue((seasonId, week.UserId), out ArenaEntry? entry))
                    {
                        week.FinalPoint = entry.Points;
                        week.FinalRank = ranks.GetValueOrDefault(week.UserId);
                    }
                    week.Closed = true;
                    changes.SavedWeeks.Add(week);
                }
                continue;
            }

            foreach (ArenaEntry player in _entries.Values.Where(e => e.SeasonId == seasonId && !e.IsBot && e.SeasonFinalRank == 0))
            {
                player.SeasonFinalRank = ranks.GetValueOrDefault(player.UserId, 1);
                player.MatchingUserIds = [];
                changes.SavedEntries.Add(player);
            }
            foreach (ArenaEntry bot in _entries.Values.Where(e => e.SeasonId == seasonId && e.IsBot).ToList())
            {
                _entries.Remove((seasonId, bot.UserId));
            }
            foreach (ArenaBattle battle in _battles.Values.Where(b => b.SeasonId == seasonId).ToList())
            {
                _battles.Remove(battle.BattleId);
            }
            changes.ClearedSeasons.Add(seasonId);
        }
    }

    private ArenaOutcome CanStartLocked(int seasonId, long userId, long opponentUserId)
    {
        if (!_entries.TryGetValue((seasonId, userId), out ArenaEntry? entry))
        {
            return ArenaOutcome.NotEntered;
        }
        return entry.MatchingUserIds.Contains(opponentUserId) && _entries.ContainsKey((seasonId, opponentUserId))
            ? ArenaOutcome.Ok
            : ArenaOutcome.NotInMatchingList;
    }

    /// <summary>Applies a points change (never below 0) and returns the change that actually happened.</summary>
    private static int Move(ArenaEntry entry, int change, long nowMs)
    {
        int before = entry.Points;
        entry.Points = Math.Max(entry.Points + change, 0);
        if (entry.Points != before)
        {
            entry.PointsChangedAt = nowMs;
        }
        return entry.Points - before;
    }

    private static bool SetDefense(ArenaEntry entry, ArenaDefense defense)
    {
        if (entry.DefenseDeckPower == defense.DeckPower && entry.MostPowerfulCostumeId == defense.MostPowerfulCostumeId
            && entry.DefenseCostumeIds.SequenceEqual(defense.CostumeIds) && entry.DefenseAttributeTypes.SequenceEqual(defense.AttributeTypes))
        {
            return false;
        }

        entry.DefenseDeckPower = defense.DeckPower;
        entry.DefenseCostumeIds = [.. defense.CostumeIds];
        entry.DefenseAttributeTypes = [.. defense.AttributeTypes];
        entry.MostPowerfulCostumeId = defense.MostPowerfulCostumeId;
        return true;
    }

    /// <summary>The season's players and bots by place: most points first, then whoever reached them first.</summary>
    private List<ArenaEntry> Ranked(int seasonId)
        => [.. _entries.Values
            .Where(e => e.SeasonId == seasonId)
            .OrderByDescending(e => e.Points)
            .ThenBy(e => e.PointsChangedAt)
            .ThenBy(e => e.UserId)];

    private Dictionary<long, int> Ranks(int seasonId)
        => Ranked(seasonId).Select((e, i) => (e.UserId, Rank: i + 1)).ToDictionary(r => r.UserId, r => r.Rank);

    private static ArenaStanding StandingOf(ArenaEntry entry, Dictionary<long, int> ranks)
        => StandingOf(entry, ranks.GetValueOrDefault(entry.UserId));

    private static ArenaStanding StandingOf(ArenaEntry e, int rank) => new(
        e.UserId, e.PlayerId, e.IsBot, e.BotName, e.BotSeed, e.BotFavoriteCostumeId,
        e.Points, rank, new ArenaDefense(e.DefenseDeckPower, [.. e.DefenseCostumeIds], [.. e.DefenseAttributeTypes], e.MostPowerfulCostumeId),
        e.AttackWins, e.AttackLosses, e.AttackPointsChange, e.DefenseWins, e.DefenseLosses, e.DefensePointsChange,
        e.SeasonFinalRank);
}
