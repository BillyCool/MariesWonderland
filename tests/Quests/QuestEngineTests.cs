using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Quests;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;

namespace MariesWonderland.Tests.Quests;

/// <summary>
/// Shared quest rules against real master data. The quest under test is the first one that has
/// first-clear rewards, gold, user EXP and a big-win (COMPLETE) mission, so every reward path runs.
/// </summary>
public class QuestEngineTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture)
{
    // Before any campaign, so drops are the quest's base drops.
    private const long NowMs = 0;

    private QuestEngine CreateEngine(bool freeStamina = false)
        => new(MasterDb, GameConfig, new QuestCampaignHelper(MasterDb), new ImportantItemEffectHelper(MasterDb), new MissionTracker(MasterDb, new MissionCatalog(MasterDb, MissionFilterTable.Empty)), Options.Create(new DevModeOptions { FreeStamina = freeStamina }));

    private EntityMQuest FindRewardingQuest()
    {
        HashSet<int> bigWinMissionIds = [.. MasterDb.EntityMQuestMission
            .Where(m => m.QuestMissionConditionType == QuestMissionConditionType.COMPLETE)
            .Select(m => m.QuestMissionId)];
        HashSet<int> groupsWithBigWin = [.. MasterDb.EntityMQuestMissionGroup
            .Where(g => bigWinMissionIds.Contains(g.QuestMissionId))
            .Select(g => g.QuestMissionGroupId)];

        return MasterDb.EntityMQuest
            .Where(q => q.QuestFirstClearRewardGroupId != 0 && q.Gold > 0 && q.UserExp > 0 && groupsWithBigWin.Contains(q.QuestMissionGroupId))
            .OrderBy(q => q.QuestId)
            .First();
    }

    /// <summary>A newly registered user, seeded the same way as a real account.</summary>
    private DarkUserMemoryDatabase CreateUser()
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("quest-engine-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        return userDb;
    }

    private long GoldCount(DarkUserMemoryDatabase userDb)
        => userDb.EntityIUserConsumableItem.FirstOrDefault(c => c.ConsumableItemId == GameConfig.ConsumableItemIdForGold)?.Count ?? 0;

    [Fact]
    public void EnsureQuestRows_CreatesQuestAndMissionRowsOnce()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();

        engine.EnsureQuestRows(userDb, quest.QuestId);
        engine.EnsureQuestRows(userDb, quest.QuestId);

        int missionCount = MasterDb.EntityMQuestMissionGroup.Count(g => g.QuestMissionGroupId == quest.QuestMissionGroupId);
        Assert.Single(userDb.EntityIUserQuest, q => q.QuestId == quest.QuestId);
        Assert.Equal(missionCount, userDb.EntityIUserQuestMission.Count(m => m.QuestId == quest.QuestId));
    }

    [Fact]
    public void Finish_FirstWin_GrantsFirstClearRewardsAndClears()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();
        EntityIUserQuest userQuest = engine.EnsureQuestRows(userDb, quest.QuestId);
        int expBefore = userDb.EntityIUserStatus[0].Exp;
        long goldBefore = GoldCount(userDb);

        QuestFinishResult result = engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs);

        Assert.Equal((int)QuestStateType.CLEARED, userQuest.QuestStateType);
        Assert.Equal(1, userQuest.ClearCount);
        Assert.NotEmpty(result.FirstClearRewards);
        Assert.True(result.IsBigWin);
        Assert.NotEqual(0, result.BigWinMissionId);
        Assert.Equal(expBefore + quest.UserExp, userDb.EntityIUserStatus[0].Exp);
        Assert.True(GoldCount(userDb) - goldBefore >= quest.Gold);
        Assert.All(userDb.EntityIUserQuestMission.Where(m => m.QuestId == quest.QuestId), m => Assert.True(m.IsClear));
    }

    [Fact]
    public void Finish_WithTheForceBonus_ClearsEveryMissionWithoutCheckingItsCondition()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = MasterDb.EntityMQuest
            .Where(q => q.IsBigWinTarget && q.QuestFirstClearRewardGroupId != 0 && q.Gold > 0 && q.UserExp > 0
                && MasterDb.EntityMQuestMissionGroup.Any(g => g.QuestMissionGroupId == q.QuestMissionGroupId))
            .OrderBy(q => q.QuestId)
            .First();
        int bonusPower = int.Parse(MasterDb.EntityMConfig.Single(c => c.ConfigKey == "QUEST_MISSION_BIG_WIN_BONUS_POWER").Value);

        QuestFinishResult Run(int deckPower, out DarkUserMemoryDatabase userDb)
        {
            userDb = CreateUser();
            engine.EnsureQuestRows(userDb, quest.QuestId);
            long userId = userDb.EntityIUserStatus[0].UserId;
            userDb.EntityIUserDeck.Add(new EntityIUserDeck { UserId = userId, DeckType = DeckType.QUEST, UserDeckNumber = 1, Power = deckPower });
            userDb.EntitySQuestSession.Add(new EntitySQuestSession { UserId = userId, QuestId = quest.QuestId, UserDeckNumber = 1, UserDeckType = DeckType.QUEST });
            return engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs, battles: []);
        }

        QuestFinishResult weak = Run(quest.RecommendedDeckPower + bonusPower - 1, out DarkUserMemoryDatabase weakUser);
        QuestFinishResult strong = Run(quest.RecommendedDeckPower + bonusPower, out DarkUserMemoryDatabase strongUser);

        HashSet<int> unmetWhenWeak = [.. weakUser.EntityIUserQuestMission.Where(m => m.QuestId == quest.QuestId && !m.IsClear).Select(m => m.QuestMissionId)];
        Assert.NotEmpty(unmetWhenWeak);
        Assert.All(strongUser.EntityIUserQuestMission.Where(m => m.QuestId == quest.QuestId), m => Assert.True(m.IsClear));
        Assert.Equal(unmetWhenWeak.Order(), strong.BigWinClearedMissionIds.Where(unmetWhenWeak.Contains).Order());
        Assert.DoesNotContain(weak.BigWinClearedMissionIds, unmetWhenWeak.Contains);
        Assert.True(strong.IsBigWin);
    }

    [Fact]
    public void Finish_Win_KeepsTheFastestRunWithAllWavesAddedUp()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();
        EntityIUserQuest userQuest = engine.EnsureQuestRows(userDb, quest.QuestId);
        static EntitySBattleDetail[] Waves(params long[] frames) => [.. frames.Select(f => new EntitySBattleDetail { ElapsedFrameCount = f })];

        engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs, battles: Waves(2000, 2500, 2300));
        Assert.Equal(6800, userQuest.ShortestClearFrames);

        engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs, battles: Waves(3000, 4000));
        Assert.Equal(6800, userQuest.ShortestClearFrames);

        engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs, battles: Waves(3000, 3500));
        Assert.Equal(6500, userQuest.ShortestClearFrames);
    }

    [Fact]
    public void Finish_Win_WithoutBattles_LeavesTheClearTime()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();
        EntityIUserQuest userQuest = engine.EnsureQuestRows(userDb, quest.QuestId);

        engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs);

        Assert.Equal(0, userQuest.ShortestClearFrames);
    }

    [Fact]
    public void Finish_SecondWin_GrantsExpAndGoldButNoOneTimeRewards()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();
        EntityIUserQuest userQuest = engine.EnsureQuestRows(userDb, quest.QuestId);

        engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs);
        int expAfterFirst = userDb.EntityIUserStatus[0].Exp;
        long goldAfterFirst = GoldCount(userDb);
        QuestFinishResult second = engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs);

        Assert.Empty(second.FirstClearRewards);
        Assert.Empty(second.MissionClearRewards);
        Assert.False(second.IsBigWin);
        Assert.Equal(2, userQuest.ClearCount);
        Assert.Equal(expAfterFirst + quest.UserExp, userDb.EntityIUserStatus[0].Exp);
        Assert.True(GoldCount(userDb) - goldAfterFirst >= quest.Gold);
    }

    [Fact]
    public void Finish_Win_GrantsMissionAndBigWinRewards()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();
        engine.EnsureQuestRows(userDb, quest.QuestId);
        int gemsBefore = userDb.EntityIUserGem[0].FreeGem;

        QuestFinishResult result = engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs);

        // Every free-gem reward in the result was granted, mission and big-win rewards included.
        int listedGems = result.FirstClearRewards
            .Concat(result.MissionClearRewards)
            .Concat(result.MissionClearCompleteRewards)
            .Concat(result.DropRewards)
            .Where(r => r.PossessionType == (int)PossessionType.FREE_GEM)
            .Sum(r => r.Count);
        Assert.NotEmpty(result.MissionClearRewards.Concat(result.MissionClearCompleteRewards));
        Assert.True(result.MissionClearRewards.Concat(result.MissionClearCompleteRewards).Any(r => r.PossessionType == (int)PossessionType.FREE_GEM));
        Assert.Equal(gemsBefore + listedGems, userDb.EntityIUserGem[0].FreeGem);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Finish_Loss_GrantsNothingAndLeavesMissions(bool isRetired, bool isAnnihilated)
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();
        EntityIUserQuest userQuest = engine.EnsureQuestRows(userDb, quest.QuestId);
        QuestEngine.MarkActive(userQuest, NowMs);
        int expBefore = userDb.EntityIUserStatus[0].Exp;
        long goldBefore = GoldCount(userDb);

        QuestFinishResult result = engine.Finish(userDb, quest, MainTarget(quest), isWin: !isRetired && !isAnnihilated, NowMs);

        Assert.Equal((int)QuestStateType.ACTIVE, userQuest.QuestStateType);
        Assert.Equal(0, userQuest.ClearCount);
        Assert.Equal(expBefore, userDb.EntityIUserStatus[0].Exp);
        Assert.Equal(goldBefore, GoldCount(userDb));
        Assert.Empty(result.FirstClearRewards);
        Assert.Empty(result.DropRewards);
        Assert.All(userDb.EntityIUserQuestMission.Where(m => m.QuestId == quest.QuestId), m => Assert.False(m.IsClear));
    }

    [Fact]
    public void Finish_LossOfAClearedQuest_RestoresCleared()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();
        EntityIUserQuest userQuest = engine.EnsureQuestRows(userDb, quest.QuestId);
        engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs);
        QuestEngine.MarkActive(userQuest, NowMs);

        engine.Finish(userDb, quest, MainTarget(quest), isWin: false, NowMs);

        Assert.Equal((int)QuestStateType.CLEARED, userQuest.QuestStateType);
        Assert.Equal(1, userQuest.ClearCount);
    }

    [Fact]
    public void Finish_UserLevelUp_RefillsStaminaToNewMax()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();
        engine.EnsureQuestRows(userDb, quest.QuestId);
        EntityIUserStatus status = userDb.EntityIUserStatus[0];
        status.Level = 1;
        status.Exp = UserLevelThreshold(2) - 1;
        status.StaminaMilliValue = 0;

        engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs);

        Assert.True(status.Level >= 2);
        Assert.Equal(MasterDb.EntityMUserLevel.Find(status.Level)!.MaxStamina * 1000, status.StaminaMilliValue);
    }

    [Fact]
    public void Finish_AtMaxLevel_CapsUserExp()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();
        engine.EnsureQuestRows(userDb, quest.QuestId);
        int maxLevelExp = MasterDb.EntityMNumericalParameterMap
            .Where(r => r.NumericalParameterMapId == LevelHelper.UserExpMapId)
            .Max(r => r.ParameterValue);
        EntityIUserStatus status = userDb.EntityIUserStatus[0];
        (status.Level, status.Exp) = LevelHelper.ApplyExp(MasterDb, LevelHelper.UserExpMapId, maxLevelExp);
        int maxLevel = status.Level;

        engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs);

        Assert.Equal(maxLevelExp, status.Exp);
        Assert.Equal(maxLevel, status.Level);
    }

    [Fact]
    public void Finish_ReplayWin_GrantsNoFirstClear_AndPaysTheMainQuestsUnclearedMissionsOnce()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindReplayQuest();
        int mainQuestId = MasterDb.EntityMQuestRelationMainFlow.First(r => r.ReplayFlowQuestId == quest.QuestId).MainFlowQuestId;
        DarkUserMemoryDatabase userDb = CreateUser();
        EntityIUserQuest userQuest = QuestEngine.GetOrCreateUserQuest(userDb, quest.QuestId);
        QuestEngine.MarkActive(userQuest, NowMs);
        int expBefore = userDb.EntityIUserStatus[0].Exp;

        QuestFinishResult result = engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs, isReplay: true);

        Assert.Equal((int)QuestStateType.CLEARED, userQuest.QuestStateType);
        Assert.Equal(1, userQuest.ClearCount);
        Assert.Equal(expBefore + quest.UserExp, userDb.EntityIUserStatus[0].Exp);
        Assert.Empty(result.FirstClearRewards);
        Assert.NotEmpty(result.MissionClearRewards);
        Assert.DoesNotContain(userDb.EntityIUserQuestMission, m => m.QuestId == quest.QuestId);
        Assert.Contains(userDb.EntityIUserQuestMission, m => m.QuestId == mainQuestId && m.IsClear);

        QuestFinishResult again = engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs, isReplay: true);

        Assert.Empty(again.MissionClearRewards);
        Assert.Empty(again.MissionClearCompleteRewards);
        Assert.False(again.IsBigWin);
    }

    [Fact]
    public void IsPlayable_BackgroundQuestWithBattleIsPlayed()
    {
        EntityMQuest battle = MasterDb.EntityMQuest.First(q => q.IsRunInTheBackground && q.RecommendedDeckPower > 0);
        EntityMQuest cutscene = MasterDb.EntityMQuest.First(q => q.IsRunInTheBackground && q.RecommendedDeckPower == 0);

        Assert.True(QuestEngine.IsPlayable(battle));
        Assert.False(QuestEngine.IsPlayable(cutscene));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConsumeStamina_SpendsTheQuestCostUnlessFreeStamina(bool freeStamina)
    {
        QuestEngine engine = CreateEngine(freeStamina);
        EntityMQuest quest = MasterDb.EntityMQuest.Where(q => q.Stamina > 0).OrderBy(q => q.QuestId).First();
        DarkUserMemoryDatabase userDb = CreateUser();
        EntityIUserStatus status = userDb.EntityIUserStatus[0];
        status.StaminaMilliValue = 100_000;
        status.StaminaUpdateDatetime = NowMs;

        engine.ConsumeStamina(userDb, quest, MainTarget(quest), 2, NowMs);

        Assert.Equal(freeStamina ? 100_000 : 100_000 - quest.Stamina * 2 * 1000, status.StaminaMilliValue);
    }

    [Fact]
    public void Finish_Win_ClearsInvisibleMissionsWithoutListingThem()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindQuestWithInvisibleMissions();
        HashSet<int> invisibleIds = InvisibleMissionIds(quest);
        DarkUserMemoryDatabase userDb = CreateUser();
        engine.EnsureQuestRows(userDb, quest.QuestId);

        QuestFinishResult result = engine.Finish(userDb, quest, MainTarget(quest), isWin: true, NowMs);

        Assert.All(invisibleIds, id => Assert.Contains(userDb.EntityIUserQuestMission, m => m.QuestId == quest.QuestId && m.QuestMissionId == id && m.IsClear));
        Assert.Equal(MasterDb.EntityMQuestMissionGroup.Count(g => g.QuestMissionGroupId == quest.QuestMissionGroupId), result.MissionClearRewards.Count + result.MissionClearCompleteRewards.Count);
    }

    private EntityMQuest FindQuestWithInvisibleMissions()
        => MasterDb.EntityMQuest
            .Where(q => q.InvisibleQuestMissionGroupId != 0 && q.QuestMissionGroupId != 0)
            .OrderBy(q => q.QuestId)
            .First();

    private HashSet<int> InvisibleMissionIds(EntityMQuest quest)
        => [.. MasterDb.EntityMQuestMissionGroup
            .Where(g => g.QuestMissionGroupId == quest.InvisibleQuestMissionGroupId)
            .Select(g => g.QuestMissionId)];

    private EntityMQuest FindReplayQuest()
        => MasterDb.EntityMQuest
            .Where(q => q.QuestReplayFlowRewardGroupId != 0 && q.QuestFirstClearRewardGroupId != 0 && q.QuestMissionGroupId != 0 && q.UserExp > 0)
            .OrderBy(q => q.QuestId)
            .First();

    private static QuestCampaignTarget MainTarget(EntityMQuest quest) => new(quest.QuestId, QuestType.MAIN_QUEST);

    private int UserLevelThreshold(int level)
        => MasterDb.EntityMNumericalParameterMap
            .Single(r => r.NumericalParameterMapId == LevelHelper.UserExpMapId && r.ParameterKey == level)
            .ParameterValue;

    [Fact]
    public void LevelForExp_UsesHighestThresholdReached()
    {
        List<EntityMNumericalParameterMap> thresholds = [.. MasterDb.EntityMNumericalParameterMap
            .Where(r => r.NumericalParameterMapId == LevelHelper.UserExpMapId)
            .OrderBy(r => r.ParameterKey)];
        EntityMNumericalParameterMap level5 = thresholds.Single(r => r.ParameterKey == 5);

        Assert.Equal(1, LevelHelper.LevelForExp(MasterDb, LevelHelper.UserExpMapId, 0));
        Assert.Equal(5, LevelHelper.LevelForExp(MasterDb, LevelHelper.UserExpMapId, level5.ParameterValue));
        Assert.Equal(4, LevelHelper.LevelForExp(MasterDb, LevelHelper.UserExpMapId, level5.ParameterValue - 1));
    }

    [Fact]
    public void DailyClearCount_StartsAgainOnANewGameDay()
    {
        long today = new DateTimeOffset(2026, 10, 25, 12, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        long yesterday = today - (long)TimeSpan.FromDays(1).TotalMilliseconds;
        EntityIUserQuest staleQuest = new() { QuestId = 1, DailyClearCount = 3, ClearCount = 10, LastClearDatetime = yesterday };
        EntityIUserQuest clearedToday = new() { QuestId = 2, DailyClearCount = 3, ClearCount = 10, LastClearDatetime = today };
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserQuest.AddRange([staleQuest, clearedToday]);

        QuestEngine.ResetDailyClearCounts(userDb, today);

        Assert.Equal(0, staleQuest.DailyClearCount);
        Assert.Equal(3, clearedToday.DailyClearCount);

        QuestEngine.MarkCleared(staleQuest, 1, today);
        Assert.Equal(1, staleQuest.DailyClearCount);
        Assert.Equal(11, staleQuest.ClearCount);
    }
}
