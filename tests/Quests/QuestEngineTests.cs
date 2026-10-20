using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Quests;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Quests;

/// <summary>
/// Shared quest rules against real master data. The quest under test is the first one that has
/// first-clear rewards, gold, user EXP and a big-win (COMPLETE) mission, so every reward path runs.
/// </summary>
public class QuestEngineTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture)
{
    // Before any campaign, so drops are the quest's base drops.
    private const long NowMs = 0;

    private QuestEngine CreateEngine() => new(MasterDb, GameConfig, new QuestCampaignHelper(MasterDb));

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

        QuestFinishResult result = engine.Finish(userDb, quest, new QuestCampaignTarget(quest.QuestId, QuestType.MAIN_QUEST), isRetired: false, NowMs);

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
    public void Finish_SecondWin_GivesNoFirstClearMissionOrBigWinRewards()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();
        EntityIUserQuest userQuest = engine.EnsureQuestRows(userDb, quest.QuestId);
        QuestCampaignTarget target = new(quest.QuestId, QuestType.MAIN_QUEST);

        engine.Finish(userDb, quest, target, isRetired: false, NowMs);
        int expAfterFirst = userDb.EntityIUserStatus[0].Exp;
        QuestFinishResult second = engine.Finish(userDb, quest, target, isRetired: false, NowMs);

        Assert.Empty(second.FirstClearRewards);
        Assert.Empty(second.MissionClearRewards);
        Assert.False(second.IsBigWin);
        Assert.Equal(2, userQuest.ClearCount);
        Assert.Equal(expAfterFirst, userDb.EntityIUserStatus[0].Exp);
    }

    [Fact]
    public void Finish_Retired_GrantsNothingAndDoesNotClear()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();
        EntityIUserQuest userQuest = engine.EnsureQuestRows(userDb, quest.QuestId);
        QuestEngine.MarkActive(userQuest, NowMs);
        int expBefore = userDb.EntityIUserStatus[0].Exp;
        long goldBefore = GoldCount(userDb);

        engine.Finish(userDb, quest, new QuestCampaignTarget(quest.QuestId, QuestType.MAIN_QUEST), isRetired: true, NowMs);

        Assert.Equal((int)QuestStateType.ACTIVE, userQuest.QuestStateType);
        Assert.Equal(0, userQuest.ClearCount);
        Assert.Equal(expBefore, userDb.EntityIUserStatus[0].Exp);
        Assert.Equal(goldBefore, GoldCount(userDb));
    }

    [Fact]
    public void ResetMissionProgress_UnclearsTheQuestsMissions()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = FindRewardingQuest();
        DarkUserMemoryDatabase userDb = CreateUser();
        engine.ClearQuestMissions(userDb, quest.QuestId, NowMs);

        QuestEngine.ResetMissionProgress(userDb, quest.QuestId);

        List<EntityIUserQuestMission> missions = [.. userDb.EntityIUserQuestMission.Where(m => m.QuestId == quest.QuestId)];
        Assert.NotEmpty(missions);
        Assert.All(missions, m => Assert.False(m.IsClear));
    }

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
}
