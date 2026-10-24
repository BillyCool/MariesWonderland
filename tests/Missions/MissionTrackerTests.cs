using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Missions;

/// <summary>Mission engine (milestone 1) against real master data and the checked-in filter table.</summary>
public class MissionTrackerTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    // 2026-10-21 12:00 UTC: permanent (challenge, evaluate) missions and the current daily set are in term.
    private static readonly long NowMs = new DateTimeOffset(2026, 10, 21, 12, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();

    private static MissionFilterTable Filters => MissionFilterTable.Load(MissionFilterTable.DefaultPath);

    private MissionCatalog Catalog => new(MasterDb, Filters);

    private (MissionTracker Tracker, MissionCatalog Catalog, DarkUserMemoryDatabase UserDb) Create()
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("mission-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        userDb.EntityIUserMission.Clear();
        MissionCatalog catalog = Catalog;
        return (new MissionTracker(MasterDb, catalog), catalog, userDb);
    }

    private static EntityIUserMission Row(DarkUserMemoryDatabase userDb, int missionId)
        => userDb.EntityIUserMission.Single(r => r.MissionId == missionId);

    [Fact]
    public void FilterTable_MapsEveryExaltAndBossMission_ToRealMasterData()
    {
        MissionFilterTable filters = Filters;
        HashSet<int> characterIds = [.. MasterDb.EntityMCharacter.Select(c => c.CharacterId)];
        HashSet<int> bossIds = [.. MasterDb.EntityMBigHuntBoss.Select(b => b.BigHuntBossId)];

        foreach (MissionDefinition mission in Catalog.ByConditionType(MissionClearConditionType.CHARACTER_REBIRTH_COUNT))
        {
            Assert.False(mission.IsFilterUnmapped, $"mission {mission.MissionId}");
            Assert.Contains(mission.Filter!.CharacterId!.Value, characterIds);
        }

        foreach (MissionDefinition mission in Catalog.ByConditionType(MissionClearConditionType.BIG_HUNT_HIGH_SCORE).Where(m => m.Filter != null))
        {
            Assert.Contains(mission.Filter!.BigHuntBossId!.Value, bossIds);
        }

        Assert.True(filters.Count >= 38);
    }

    [Fact]
    public void Refresh_CreatesAvailableMissions_AndClearsStateConditionsAlreadyMet()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        EntityIUserStatus status = userDb.EntityIUserStatus.Single();
        status.Level = 30;

        tracker.Refresh(userDb, NowMs);

        List<MissionDefinition> levelMissions = [.. catalog.ByConditionType(MissionClearConditionType.USER_LEVEL)
            .Where(m => m.IsInTerm(NowMs) && m.UnlockType == MissionUnlockConditionType.UNKNOWN)];
        Assert.NotEmpty(levelMissions);
        foreach (MissionDefinition mission in levelMissions)
        {
            EntityIUserMission row = Row(userDb, mission.MissionId);
            Assert.Equal(mission.ClearValue <= 30 ? MissionProgressStatusType.CLEAR : MissionProgressStatusType.IN_PROGRESS, row.MissionProgressStatusType);
            Assert.Equal(NowMs, row.StartDatetime);
        }

        // Counters never count earlier activity: a fresh row starts at 0.
        MissionDefinition counter = catalog.ByConditionType(MissionClearConditionType.QUEST_CLEAR_BY_COUNT)
            .First(m => m.IsInTerm(NowMs) && m.UnlockType == MissionUnlockConditionType.UNKNOWN && m.Category == MissionCategoryType.CHALLENGE);
        Assert.Equal(0, Row(userDb, counter.MissionId).ProgressValue);

        // Shop-granted and out-of-term missions get no row.
        Assert.DoesNotContain(userDb.EntityIUserMission, r => catalog.Find(r.MissionId)!.UnlockType == MissionUnlockConditionType.GRANT);
        Assert.DoesNotContain(userDb.EntityIUserMission, r => !catalog.Find(r.MissionId)!.IsInTerm(NowMs));
    }

    [Fact]
    public void Refresh_HiddenStoryMissionsAreAvailable_AndQuestUnlockedOnesWaitForTheQuest()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        MissionDefinition questLocked = catalog.All.First(m => m.UnlockType == MissionUnlockConditionType.QUEST_CLEAR && m.IsInTerm(NowMs));
        userDb.EntityIUserQuest.RemoveAll(q => q.QuestId == questLocked.UnlockValue);

        tracker.Refresh(userDb, NowMs);

        Assert.Equal(catalog.All.Count(m => m.Category == MissionCategoryType.EVALUATE),
            userDb.EntityIUserMission.Count(r => catalog.Find(r.MissionId)!.Category == MissionCategoryType.EVALUATE));
        Assert.DoesNotContain(userDb.EntityIUserMission, r => r.MissionId == questLocked.MissionId);

        userDb.EntityIUserQuest.Add(new EntityIUserQuest { UserId = userDb.UserId, QuestId = questLocked.UnlockValue, ClearCount = 1 });
        tracker.Refresh(userDb, NowMs);

        Assert.Contains(userDb.EntityIUserMission, r => r.MissionId == questLocked.MissionId);
    }

    [Fact]
    public void Record_CountsOnlyMatchingFilters_AndClearsAtTheTarget()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        MissionDefinition mission = catalog.ByConditionType(MissionClearConditionType.QUEST_CLEAR_BY_COUNT)
            .First(m => m.IsInTerm(NowMs) && m.UnlockType == MissionUnlockConditionType.UNKNOWN && m.Filter == null && !m.IsFilterUnmapped && m.ClearValue > 1);
        tracker.Refresh(userDb, NowMs);
        EntityIUserMission row = Row(userDb, mission.MissionId);

        tracker.Record(userDb, MissionClearConditionType.QUEST_CLEAR_BY_COUNT, 1, _ => false, NowMs);
        Assert.Equal(0, row.ProgressValue);

        tracker.Record(userDb, MissionClearConditionType.QUEST_CLEAR_BY_COUNT, mission.ClearValue - 1, _ => true, NowMs);
        Assert.Equal(MissionProgressStatusType.IN_PROGRESS, row.MissionProgressStatusType);

        tracker.Record(userDb, MissionClearConditionType.QUEST_CLEAR_BY_COUNT, 2, _ => true, NowMs);
        Assert.Equal(MissionProgressStatusType.CLEAR, row.MissionProgressStatusType);
        Assert.Equal(NowMs, row.ClearDatetime);
        // Progress keeps counting past the target until the reward is received, as in the real save.
        Assert.Equal(mission.ClearValue + 1, row.ProgressValue);
    }

    [Fact]
    public void MissionsClearing_CountTowardsCompleteNMissions()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        tracker.Refresh(userDb, NowMs);
        MissionDefinition completeN = catalog.ByConditionType(MissionClearConditionType.MISSION_CLEAR_BY_COUNT).First(m => m.IsInTerm(NowMs));
        EntityIUserMission completeRow = userDb.EntityIUserMission.FirstOrDefault(r => r.MissionId == completeN.MissionId)
            ?? throw new InvalidOperationException("complete-N mission not available");
        int before = completeRow.ProgressValue;
        int counterMissions = userDb.EntityIUserMission.Count(r => r.MissionProgressStatusType == MissionProgressStatusType.IN_PROGRESS
            && catalog.Find(r.MissionId)!.ConditionType == MissionClearConditionType.QUEST_CLEAR_BY_COUNT
            && !catalog.Find(r.MissionId)!.IsFilterUnmapped
            && catalog.Find(r.MissionId)!.ClearValue == 1);

        tracker.Record(userDb, MissionClearConditionType.QUEST_CLEAR_BY_COUNT, 1, _ => true, NowMs);

        Assert.True(counterMissions > 0);
        Assert.True(completeRow.ProgressValue >= before + counterMissions);
    }

    [Fact]
    public void TotalDaysLoginMissions_CountLoginDaysBeforeTheMissionExisted_AndDailiesStayAtZero()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        userDb.EntityIUserLogin.Single().TotalLoginCount = 2;

        tracker.Refresh(userDb, NowMs);

        MissionDefinition oneDay = catalog.Find(940001)!;
        MissionDefinition twoDays = catalog.Find(940002)!;
        MissionDefinition threeDays = catalog.Find(940003)!;
        Assert.Equal(MissionProgressStatusType.CLEAR, Row(userDb, oneDay.MissionId).MissionProgressStatusType);
        Assert.Equal(MissionProgressStatusType.CLEAR, Row(userDb, twoDays.MissionId).MissionProgressStatusType);
        Assert.Equal(2, Row(userDb, threeDays.MissionId).ProgressValue);
        Assert.Equal(MissionProgressStatusType.IN_PROGRESS, Row(userDb, threeDays.MissionId).MissionProgressStatusType);
        Assert.All(userDb.EntityIUserMission.Where(r => catalog.Find(r.MissionId)!.IsDaily
            && catalog.Find(r.MissionId)!.ConditionType == MissionClearConditionType.USER_LOGIN_BY_COUNT),
            r => Assert.Equal(0, r.ProgressValue));
    }

    [Fact]
    public void DailyMissions_StartOverOnANewGameDay()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        tracker.Refresh(userDb, NowMs);
        EntityIUserMission daily = userDb.EntityIUserMission.First(r => catalog.Find(r.MissionId)!.IsDaily
            && catalog.Find(r.MissionId)!.ConditionType == MissionClearConditionType.USER_LOGIN_BY_COUNT);
        daily.ProgressValue = 1;
        daily.MissionProgressStatusType = MissionProgressStatusType.REWARD_RECEIVED;

        long tomorrow = GameDayHelper.StartOfDay(NowMs) + (long)TimeSpan.FromDays(1).TotalMilliseconds;
        tracker.Refresh(userDb, tomorrow - 1);
        Assert.Equal(MissionProgressStatusType.REWARD_RECEIVED, daily.MissionProgressStatusType);

        tracker.Refresh(userDb, tomorrow);
        Assert.Equal(MissionProgressStatusType.IN_PROGRESS, daily.MissionProgressStatusType);
        Assert.Equal(0, daily.ProgressValue);
        Assert.Equal(tomorrow, daily.StartDatetime);
    }

    [Fact]
    public void ReceiveRewards_GrantsClearedMissionsOnce_AndReturnsEndedTermsAsExpired()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        userDb.EntityIUserStatus.Single().Level = 30;
        tracker.Refresh(userDb, NowMs);
        MissionDefinition cleared = catalog.ByConditionType(MissionClearConditionType.USER_LEVEL)
            .First(m => m.IsInTerm(NowMs) && m.ClearValue <= 30 && m.Rewards.Count > 0 && m.UnlockType == MissionUnlockConditionType.UNKNOWN);
        EntityMMissionReward reward = cleared.Rewards[0];

        (List<MissionRewardGrant> received, List<MissionRewardGrant> expired) = tracker.ReceiveRewards(userDb, [cleared.MissionId, cleared.MissionId], NowMs);
        Assert.Equal([new MissionRewardGrant(reward.PossessionType, reward.PossessionId, reward.Count)], received);
        Assert.Empty(expired);
        Assert.Equal(MissionProgressStatusType.REWARD_RECEIVED, Row(userDb, cleared.MissionId).MissionProgressStatusType);

        (received, _) = tracker.ReceiveRewards(userDb, [cleared.MissionId], NowMs);
        Assert.Empty(received);

        // A cleared mission whose term has ended.
        MissionDefinition ended = catalog.All.First(m => !m.IsInTerm(NowMs) && m.Rewards.Count > 0);
        userDb.EntityIUserMission.Add(new EntityIUserMission
        {
            UserId = userDb.UserId,
            MissionId = ended.MissionId,
            MissionProgressStatusType = MissionProgressStatusType.CLEAR
        });
        (received, expired) = tracker.ReceiveRewards(userDb, [ended.MissionId], NowMs);
        Assert.Empty(received);
        Assert.Equal(ended.Rewards.Count, expired.Count);
        Assert.Equal(MissionProgressStatusType.REWARD_RECEIVED, Row(userDb, ended.MissionId).MissionProgressStatusType);
    }

    [Fact]
    public void GrantGroup_MakesShopMissionsAvailable()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        EntityMShopItemContentMission content = MasterDb.EntityMShopItemContentMission
            .First(c => catalog.ByGroup(c.MissionGroupId).Any(m => m.IsInTerm(NowMs)));
        tracker.Refresh(userDb, NowMs);
        Assert.DoesNotContain(userDb.EntityIUserMission, r => catalog.Find(r.MissionId)!.MissionGroupId == content.MissionGroupId);

        tracker.GrantGroup(userDb, content.MissionGroupId, NowMs);

        Assert.Equal(catalog.ByGroup(content.MissionGroupId).Count(m => m.IsInTerm(NowMs)),
            userDb.EntityIUserMission.Count(r => catalog.Find(r.MissionId)!.MissionGroupId == content.MissionGroupId));
    }

    [Fact]
    public void ExaltMissions_ReadTheFilteredCharactersRebirthCount()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        MissionDefinition exalt = catalog.ByConditionType(MissionClearConditionType.CHARACTER_REBIRTH_COUNT)
            .First(m => m.IsInTerm(NowMs) && m.ClearValue == 1);
        int characterId = exalt.Filter!.CharacterId!.Value;
        userDb.EntityIUserCharacterRebirth.RemoveAll(r => true);
        userDb.EntityIUserCharacterRebirth.Add(new EntityIUserCharacterRebirth { UserId = userDb.UserId, CharacterId = characterId + 1, RebirthCount = 5 });

        tracker.Refresh(userDb, NowMs);
        Assert.Equal(MissionProgressStatusType.IN_PROGRESS, Row(userDb, exalt.MissionId).MissionProgressStatusType);

        userDb.EntityIUserCharacterRebirth.Add(new EntityIUserCharacterRebirth { UserId = userDb.UserId, CharacterId = characterId, RebirthCount = 1 });
        tracker.Refresh(userDb, NowMs);
        Assert.Equal(MissionProgressStatusType.CLEAR, Row(userDb, exalt.MissionId).MissionProgressStatusType);
    }
}
