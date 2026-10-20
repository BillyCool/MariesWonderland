using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.BigHunt;
using MariesWonderland.Quests;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using BigHuntService = MariesWonderland.Services.BigHuntService;
using RewardService = MariesWonderland.Services.RewardService;

namespace MariesWonderland.Tests.Services;

/// <summary>
/// Big Hunt score rewards against real master data: a run grants only the highest tier its score reached, a skip
/// grants the best score's highest tier once per skip, and the daily count resets at the game day boundary.
/// </summary>
public class BigHuntRewardTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    // After every schedule and reward-group start in the data, so the latest reward group is active.
    private static readonly DateTimeOffset Now = new(2050, 1, 3, 12, 0, 0, TimeSpan.Zero);

    private (BigHuntService Service, DarkUserMemoryDatabase UserDb, long UserId) CreateService()
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("big-hunt-reward-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        // Base tier rewards unless a test adds "Daily Reward+".
        userDb.EntityIUserImportantItem.RemoveAll(i => i.ImportantItemId == 200020);
        QuestCampaignHelper campaigns = new(MasterDb);
        ImportantItemEffectHelper importantItems = new(MasterDb);
        QuestEngine engine = new(MasterDb, GameConfig, campaigns, importantItems, Options.Create(new DevModeOptions()));
        return (new BigHuntService(store, MasterDb, engine, campaigns, importantItems, new FakeTimeProvider(Now)), userDb, userId);
    }

    private EntityMBigHuntBossQuest BossQuest => MasterDb.EntityMBigHuntBossQuest.OrderBy(q => q.BigHuntBossQuestId).First();

    private List<EntityMBigHuntScoreRewardGroup> Tiers(EntityMBigHuntBossQuest bossQuest)
    {
        int groupId = MasterDb.EntityMBigHuntScoreRewardGroupSchedule
            .Where(s => s.BigHuntScoreRewardGroupScheduleId == bossQuest.BigHuntScoreRewardGroupScheduleId)
            .OrderByDescending(s => s.StartDatetime)
            .First().BigHuntScoreRewardGroupId;
        return [.. MasterDb.EntityMBigHuntScoreRewardGroup.Where(t => t.BigHuntScoreRewardGroupId == groupId).OrderBy(t => t.NecessaryScore)];
    }

    private List<string> TierItems(EntityMBigHuntScoreRewardGroup tier)
        => [.. MasterDb.EntityMBigHuntRewardGroup
            .Where(r => r.BigHuntRewardGroupId == tier.BigHuntRewardGroupId)
            .OrderBy(r => r.SortOrder)
            .Select(r => $"{(int)r.PossessionType}:{r.PossessionId}:{r.Count}")];

    private static List<string> Keys(IEnumerable<BigHuntReward> rewards)
        => [.. rewards.Select(r => $"{r.PossessionType}:{r.PossessionId}:{r.Count}")];

    private int ActiveScheduleId => MasterDb.EntityMBigHuntSchedule.OrderByDescending(s => s.ChallengeEndDatetime).First().BigHuntScheduleId;

    [Fact]
    public async Task Skip_GrantsTheBestScoresHighestTierOncePerSkip()
    {
        (BigHuntService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        EntityMBigHuntBossQuest bossQuest = BossQuest;
        List<EntityMBigHuntScoreRewardGroup> tiers = Tiers(bossQuest);
        EntityMBigHuntScoreRewardGroup tier = tiers[tiers.Count / 2];
        userDb.EntityIUserBigHuntScheduleMaxScore.Add(new EntityIUserBigHuntScheduleMaxScore
        {
            UserId = userId,
            BigHuntScheduleId = ActiveScheduleId,
            BigHuntBossId = bossQuest.BigHuntBossId,
            MaxScore = tier.NecessaryScore
        });

        SkipBigHuntQuestResponse response = await service.SkipBigHuntQuest(
            new SkipBigHuntQuestRequest { BigHuntBossQuestId = bossQuest.BigHuntBossQuestId, SkipCount = 2 }, ContextFor(userId));

        List<string> once = TierItems(tier);
        Assert.NotEmpty(once);
        Assert.Equal([.. once, .. once], Keys(response.ScoreReward));
    }

    [Fact]
    public async Task Skip_WithDailyRewardPlus_DoublesTheTierRewards()
    {
        (BigHuntService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        EntityMBigHuntBossQuest bossQuest = BossQuest;
        EntityMBigHuntScoreRewardGroup tier = Tiers(bossQuest)[^1];
        userDb.EntityIUserImportantItem.RemoveAll(i => i.ImportantItemId == 200020);
        userDb.EntityIUserImportantItem.Add(new EntityIUserImportantItem { UserId = userId, ImportantItemId = 200020, Count = 1 });
        userDb.EntityIUserBigHuntScheduleMaxScore.Add(new EntityIUserBigHuntScheduleMaxScore
        {
            UserId = userId,
            BigHuntScheduleId = ActiveScheduleId,
            BigHuntBossId = bossQuest.BigHuntBossId,
            MaxScore = tier.NecessaryScore
        });

        SkipBigHuntQuestResponse response = await service.SkipBigHuntQuest(
            new SkipBigHuntQuestRequest { BigHuntBossQuestId = bossQuest.BigHuntBossQuestId, SkipCount = 1 }, ContextFor(userId));

        List<string> doubled = [.. MasterDb.EntityMBigHuntRewardGroup
            .Where(r => r.BigHuntRewardGroupId == tier.BigHuntRewardGroupId)
            .OrderBy(r => r.SortOrder)
            .Select(r => $"{(int)r.PossessionType}:{r.PossessionId}:{r.Count * 2}")];
        Assert.Equal(doubled, Keys(response.ScoreReward));
    }

    [Fact]
    public async Task WeeklyClaim_GrantsEachAttributesHighestTier_WithoutTheDailyRewardPlusBoost()
    {
        // The user's recorded weekly claim: 250M+ on all five attributes, Daily Reward+ owned.
        (_, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        UserDataStore store = new(TimeProvider.System);
        store.Set(userId, userDb);
        userDb.EntityIUserImportantItem.Add(new EntityIUserImportantItem { UserId = userId, ImportantItemId = 200020, Count = 1 });
        long weeklyVersion = GameDayHelper.StartOfWeek(Now.ToUnixTimeMilliseconds());
        userDb.EntityIUserBigHuntWeeklyStatus.RemoveAll(s => s.BigHuntWeeklyVersion == weeklyVersion);
        foreach (EntityMBigHuntBoss boss in MasterDb.EntityMBigHuntBoss)
        {
            userDb.EntityIUserBigHuntWeeklyMaxScore.Add(new EntityIUserBigHuntWeeklyMaxScore
            {
                UserId = userId,
                BigHuntWeeklyVersion = weeklyVersion,
                AttributeType = boss.AttributeType,
                MaxScore = 250_000_000
            });
        }

        RewardService service = new(store, MasterDb, new FakeTimeProvider(Now));
        Proto.Reward.ReceiveBigHuntRewardResponse response = await service.ReceiveBigHuntReward(new Google.Protobuf.WellKnownTypes.Empty(), ContextFor(userId));

        Dictionary<string, int> totals = response.WeeklyScoreReward
            .GroupBy(r => $"{r.PossessionType}:{r.PossessionId}")
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Count));
        Dictionary<string, int> recorded = new()
        {
            ["6:37"] = 3750,     // Silver Subjugation Battle Coin
            ["6:39"] = 5250,     // Gold Subjugation Battle Coin
            ["6:144"] = 2250,    // Shiny Subjugation Battle Coin
            ["6:30"] = 4000,     // Bronze Subjugation Battle Coin
            ["5:402001"] = 1500, // Small Cursed-God Antler Bit
            ["5:402002"] = 500,  // Medium Cursed-God Antler Bit
            ["5:402003"] = 300,  // Large Cursed-God Antler Bit
            ["5:501001"] = 5,    // Piece: Zenith's Brilliance
            ["5:501002"] = 5     // Piece: Black Pearl
        };
        Assert.Equal(recorded.OrderBy(p => p.Key), totals.OrderBy(p => p.Key));
    }

    [Fact]
    public async Task Skip_WithoutAScoreThisSchedule_GrantsNothingButUsesAttempts()
    {
        (BigHuntService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        EntityMBigHuntBossQuest bossQuest = BossQuest;

        SkipBigHuntQuestResponse response = await service.SkipBigHuntQuest(
            new SkipBigHuntQuestRequest { BigHuntBossQuestId = bossQuest.BigHuntBossQuestId, SkipCount = 1 }, ContextFor(userId));

        Assert.Empty(response.ScoreReward);
        Assert.Equal(1, userDb.EntityIUserBigHuntStatus.Single(s => s.BigHuntBossQuestId == bossQuest.BigHuntBossQuestId).DailyChallengeCount);
    }

    [Fact]
    public async Task Skip_ResetsTheDailyCountOnANewGameDay()
    {
        (BigHuntService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        EntityMBigHuntBossQuest bossQuest = BossQuest;
        long lastReset = GameDayHelper.StartOfDay(Now.ToUnixTimeMilliseconds());
        userDb.EntityIUserBigHuntStatus.Add(new EntityIUserBigHuntStatus
        {
            UserId = userId,
            BigHuntBossQuestId = bossQuest.BigHuntBossQuestId,
            DailyChallengeCount = 1,
            LatestChallengeDatetime = lastReset - 1
        });

        await service.SkipBigHuntQuest(new SkipBigHuntQuestRequest { BigHuntBossQuestId = bossQuest.BigHuntBossQuestId, SkipCount = 1 }, ContextFor(userId));
        EntityIUserBigHuntStatus status = userDb.EntityIUserBigHuntStatus.Single(s => s.BigHuntBossQuestId == bossQuest.BigHuntBossQuestId);
        Assert.Equal(1, status.DailyChallengeCount);

        await service.SkipBigHuntQuest(new SkipBigHuntQuestRequest { BigHuntBossQuestId = bossQuest.BigHuntBossQuestId, SkipCount = 1 }, ContextFor(userId));
        Assert.Equal(2, status.DailyChallengeCount);
    }

    [Fact]
    public async Task Finish_GrantsOnlyTheHighestTierReached_EveryRun()
    {
        (BigHuntService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        EntityMBigHuntBossQuest bossQuest = BossQuest;
        EntityMBigHuntQuest bhQuest = MasterDb.EntityMBigHuntQuestGroup
            .Where(g => g.BigHuntQuestGroupId == bossQuest.BigHuntQuestGroupId)
            .OrderBy(g => g.SortOrder)
            .Select(g => MasterDb.EntityMBigHuntQuest.Single(q => q.BigHuntQuestId == g.BigHuntQuestId))
            .First();
        List<EntityMBigHuntScoreRewardGroup> tiers = Tiers(bossQuest);
        long damage = tiers[tiers.Count / 2].NecessaryScore;

        for (int run = 0; run < 2; run++)
        {
            await service.StartBigHuntQuest(new StartBigHuntQuestRequest
            {
                BigHuntBossQuestId = bossQuest.BigHuntBossQuestId,
                BigHuntQuestId = bhQuest.BigHuntQuestId,
                UserDeckNumber = 1
            }, ContextFor(userId));
            userDb.EntitySBigHuntSession.Single().TotalDamage = damage;

            FinishBigHuntQuestResponse response = await service.FinishBigHuntQuest(new FinishBigHuntQuestRequest
            {
                BigHuntBossQuestId = bossQuest.BigHuntBossQuestId,
                BigHuntQuestId = bhQuest.BigHuntQuestId
            }, ContextFor(userId));

            long score = response.ScoreInfo.UserScore;
            EntityMBigHuntScoreRewardGroup highest = tiers.Last(t => t.NecessaryScore <= score);
            Assert.True(tiers.Count(t => t.NecessaryScore <= score) > 1);
            Assert.Equal(TierItems(highest), Keys(response.ScoreReward));
        }
    }
}
