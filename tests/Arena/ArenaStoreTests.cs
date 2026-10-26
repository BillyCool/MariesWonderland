using MariesWonderland.Arena;

namespace MariesWonderland.Tests.Arena;

/// <summary>The Arena store on its own: entering, opponent lists, points, weekly results and season settling.</summary>
public class ArenaStoreTests
{
    private const long Alice = 1;
    private const long Bob = 2;
    private const int SeasonId = 900;

    // Monday 2026-10-19 08:00 UTC: a game week starts. The test season runs two weeks.
    private static readonly long WeekStart = new DateTimeOffset(2026, 10, 19, 8, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
    private static readonly long Day = (long)TimeSpan.FromDays(1).TotalMilliseconds;
    private static readonly ArenaSeason Season = new(SeasonId, WeekStart, WeekStart + 14 * Day - 1000);

    private static readonly ArenaDefense Deck = new(5000, [101, 102, 103], [1, 2, 3], 102);

    private readonly ArenaStore _store = new();

    private static IEnumerable<ArenaEntry> Bots(params int[] points)
        => points.Select((p, i) => new ArenaEntry
        {
            UserId = ArenaBots.IdBase + i + 1,
            PlayerId = ArenaBots.IdBase + i + 1,
            Points = p,
            BotName = $"Bot {i}",
            DefenseDeckPower = 1000,
            DefenseCostumeIds = [201],
            DefenseAttributeTypes = [1],
            MostPowerfulCostumeId = 201,
        });

    private ArenaStanding Enter(long userId, int points, long nowMs, ArenaDefense? defense = null, params int[] bots)
        => _store.Enter(Season, userId, userId * 10, points, defense ?? Deck, nowMs, () => Bots(bots), out _);

    [Fact]
    public void Enter_MakesTheBotsOnce_AndRanksByPoints()
    {
        long now = WeekStart + Day;
        ArenaStanding alice = _store.Enter(Season, Alice, 10, 19_000, Deck, now, () => Bots(20_000, 18_000), out bool isNew);
        ArenaStanding again = _store.Enter(Season, Alice, 10, 99_999, Deck, now, () => throw new InvalidOperationException("bots made twice"), out bool isNewAgain);

        Assert.True(isNew);
        Assert.False(isNewAgain);
        Assert.Equal(19_000, again.Points);
        Assert.Equal(2, alice.Rank);
        Assert.Equal(3, _store.Ranking(SeasonId, 1, 100, now).Count);
    }

    [Fact]
    public void Matching_DrawsAtRandomWithinRange()
    {
        long now = WeekStart + Day;
        Enter(Alice, 20_000, now, null, 25_000, 24_000, 21_000, 19_000, 16_000, 15_000, 14_000, 40_000);

        HashSet<long> seen = [];
        for (int seed = 0; seed < 30; seed++)
        {
            IReadOnlyList<ArenaStanding> matching = _store.NewMatching(SeasonId, Alice, new Random(seed), now);
            Assert.Equal(ArenaStore.MatchingCount, matching.Count);
            Assert.All(matching, s => Assert.InRange(s.Points, 20_000 - ArenaStore.MatchingRange, 20_000 + ArenaStore.MatchingRange));
            seen.UnionWith(matching.Select(s => s.UserId));
        }

        // Every one of the six in range turns up; 24,000 and 14,000 aren't just the nearest.
        Assert.Equal(6, seen.Count);
    }

    [Fact]
    public void Matching_FillsFromTheNearestOutOfRange_AndSkipsPlayersWithoutADefenseDeck()
    {
        long now = WeekStart + Day;
        Enter(Alice, 20_000, now, null, 40_000, 20_100, 19_900, 5_000);
        Enter(Bob, 20_000, now, ArenaDefense.None);

        IReadOnlyList<ArenaStanding> matching = _store.NewMatching(SeasonId, Alice, new Random(1), now);

        Assert.Equal(ArenaStore.MatchingCount, matching.Count);
        Assert.DoesNotContain(matching, s => s.UserId == Bob);
        Assert.Equal([20_100, 19_900, 5_000], matching.Select(s => s.Points));
        Assert.Equal(matching.OrderByDescending(s => s.Points).Select(s => s.UserId), matching.Select(s => s.UserId));
        Assert.Equal(matching.Select(s => s.UserId), _store.Matching(SeasonId, Alice, now).Select(s => s.UserId));
    }

    [Fact]
    public void Battle_MovesBothSidesPoints_AndKeepsTheWeekAndLogs()
    {
        long now = WeekStart + Day;
        Enter(Alice, 20_000, now);
        Enter(Bob, 21_000, now);
        _store.NewMatching(SeasonId, Alice, new Random(1), now);

        Assert.Equal(ArenaOutcome.Ok, _store.StartBattle(SeasonId, Alice, Bob, Deck, Deck, now));
        ArenaFinish? finish = _store.FinishBattle(SeasonId, Alice, Bob, isVictory: true, now + 1000);

        // 120 + 1000 / 20 = 170, rounded to a multiple of 12.
        Assert.NotNull(finish);
        Assert.Equal(168, finish.PointsChange);
        Assert.Equal((20_000, 2, 20_168, 2), (finish.BeforePoints, finish.BeforeRank, finish.AfterPoints, finish.AfterRank));
        Assert.Equal(21_000 - 168, _store.Standing(SeasonId, Bob, now)!.Points);
        Assert.Equal(1, _store.Standing(SeasonId, Bob, now)!.DefenseLosses);
        Assert.Null(_store.FinishBattle(SeasonId, Alice, Bob, true, now + 2000));

        ArenaLogLine attack = Assert.Single(_store.Log(SeasonId, Alice, asAttacker: true, now));
        ArenaLogLine defense = Assert.Single(_store.Log(SeasonId, Bob, asAttacker: false, now));
        Assert.Equal((Bob, true, 168), (attack.Opponent.UserId, attack.IsVictory, attack.PointsChange));
        Assert.Equal((Alice, false, -168), (defense.Opponent.UserId, defense.IsVictory, defense.PointsChange));

        ArenaWeek week = Assert.Single(_store.Weeks(Alice, now));
        Assert.Equal((WeekStart, 20_168, 2, false), (week.WeekStart, week.FinalPoint, week.FinalRank, week.Closed));
        Assert.Empty(_store.Weeks(Bob, now));
    }

    [Fact]
    public void Battle_NeedsTheOpponentOnTheList()
    {
        long now = WeekStart + Day;
        Enter(Alice, 20_000, now);
        Enter(Bob, 21_000, now);

        Assert.Equal(ArenaOutcome.NotInMatchingList, _store.StartBattle(SeasonId, Alice, Bob, Deck, Deck, now));
        Assert.Equal(ArenaOutcome.NotEntered, _store.CanStart(SeasonId, 99, Bob, now));
    }

    [Fact]
    public void Loss_CostsLessAgainstAStrongerOpponent_AndPointsNeverGoNegative()
    {
        Assert.Equal(12, ArenaPoints.LossCost(1_000, 10_000));
        Assert.Equal(60, ArenaPoints.LossCost(5_000, 5_000));
        Assert.Equal(180, ArenaPoints.LossCost(10_000, 1_000));
        Assert.Equal(360, ArenaPoints.WinGain(1_000, 10_000));
        Assert.Equal(12, ArenaPoints.WinGain(10_000, 1_000));

        long now = WeekStart + Day;
        Enter(Alice, 20, now);
        Enter(Bob, 20, now);
        _store.NewMatching(SeasonId, Alice, new Random(1), now);
        _store.StartBattle(SeasonId, Alice, Bob, Deck, Deck, now);

        ArenaFinish finish = _store.FinishBattle(SeasonId, Alice, Bob, isVictory: false, now)!;
        Assert.Equal((0, -20), (finish.AfterPoints, finish.PointsChange));
        Assert.Equal(80, _store.Standing(SeasonId, Bob, now)!.Points);
    }

    [Fact]
    public void WeekEnd_FreezesThePointsAndPlace()
    {
        long now = WeekStart + Day;
        Enter(Alice, 20_000, now);
        Enter(Bob, 21_000, now);
        _store.NewMatching(SeasonId, Alice, new Random(1), now);
        _store.StartBattle(SeasonId, Alice, Bob, Deck, Deck, now);
        _store.FinishBattle(SeasonId, Alice, Bob, isVictory: true, now);

        long nextWeek = WeekStart + 7 * Day + 1;
        ArenaWeek week = Assert.Single(_store.Weeks(Alice, nextWeek));
        Assert.True(week.Closed);
        Assert.Equal((20_168, 2), (week.FinalPoint, week.FinalRank));
    }

    [Fact]
    public void SeasonEnd_KeepsEachPlayersPlace_AndDropsTheBots()
    {
        long now = WeekStart + Day;
        Enter(Alice, 20_000, now, null, 30_000, 10_000);
        Enter(Bob, 25_000, now);

        long after = Season.EndsAt + 1;
        Assert.Equal(3, _store.Standing(SeasonId, Alice, after)!.SeasonFinalRank);
        Assert.Equal(2, _store.Standing(SeasonId, Bob, after)!.SeasonFinalRank);
        Assert.Null(_store.FindBot(ArenaBots.IdBase + 1, after));
        Assert.Equal(2, _store.Ranking(SeasonId, 1, 100, after).Count);
    }
}
