using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Gimmick;
using MariesWonderland.Tests.Infrastructure;
using GimmickServiceImpl = MariesWonderland.Services.GimmickService;

namespace MariesWonderland.Tests.Services;

public class GimmickServiceTests
{
    private const long UserId = 1L;
    private const int GoldId = 1;
    private static readonly GameConfig Config = new() { ConsumableItemIdForGold = GoldId };

    [Fact]
    public async Task UpdateSequence_NewSequence_AddsUserSequenceRecord()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        GimmickServiceImpl service = CreateService(CreateMasterDb(), userDb);

        await service.UpdateSequence(
            new UpdateSequenceRequest
            {
                GimmickSequenceScheduleId = 10,
                GimmickSequenceId = 20
            },
            FakeServerCallContext.For(UserId));

        EntityIUserGimmickSequence sequence = Assert.Single(userDb.EntityIUserGimmickSequence);
        Assert.Equal(UserId, sequence.UserId);
        Assert.Equal(10, sequence.GimmickSequenceScheduleId);
        Assert.Equal(20, sequence.GimmickSequenceId);
        Assert.False(sequence.IsGimmickSequenceCleared);
        Assert.Equal(0, sequence.ClearDatetime);
    }

    [Fact]
    public async Task UpdateSequence_ExistingSequence_DoesNotDuplicateRecord()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGimmickSequence.Add(new EntityIUserGimmickSequence
        {
            UserId = UserId,
            GimmickSequenceScheduleId = 10,
            GimmickSequenceId = 20,
            IsGimmickSequenceCleared = true,
            ClearDatetime = 12345
        });

        GimmickServiceImpl service = CreateService(CreateMasterDb(), userDb);

        await service.UpdateSequence(
            new UpdateSequenceRequest
            {
                GimmickSequenceScheduleId = 10,
                GimmickSequenceId = 20
            },
            FakeServerCallContext.For(UserId));

        EntityIUserGimmickSequence sequence = Assert.Single(userDb.EntityIUserGimmickSequence);
        Assert.True(sequence.IsGimmickSequenceCleared);
        Assert.Equal(12345, sequence.ClearDatetime);
    }

    [Fact]
    public async Task UpdateGimmickProgress_NewRecords_CreatesGimmickAndOrnamentProgress()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        GimmickServiceImpl service = CreateService(CreateMasterDb(), userDb);

        UpdateGimmickProgressResponse response = await service.UpdateGimmickProgress(
            new UpdateGimmickProgressRequest
            {
                GimmickSequenceScheduleId = 10,
                GimmickSequenceId = 20,
                GimmickId = 30,
                GimmickOrnamentIndex = 2,
                ProgressValueBit = 7
            },
            FakeServerCallContext.For(UserId));

        EntityIUserGimmick gimmick = Assert.Single(userDb.EntityIUserGimmick);
        Assert.Equal(UserId, gimmick.UserId);
        Assert.Equal(10, gimmick.GimmickSequenceScheduleId);
        Assert.Equal(20, gimmick.GimmickSequenceId);
        Assert.Equal(30, gimmick.GimmickId);
        Assert.True(gimmick.StartDatetime > 0);

        EntityIUserGimmickOrnamentProgress ornament = Assert.Single(userDb.EntityIUserGimmickOrnamentProgress);
        Assert.Equal(UserId, ornament.UserId);
        Assert.Equal(10, ornament.GimmickSequenceScheduleId);
        Assert.Equal(20, ornament.GimmickSequenceId);
        Assert.Equal(30, ornament.GimmickId);
        Assert.Equal(2, ornament.GimmickOrnamentIndex);
        Assert.Equal(7, ornament.ProgressValueBit);
        Assert.True(ornament.BaseDatetime > 0);

        Assert.False(response.IsSequenceCleared);
        Assert.Empty(response.GimmickOrnamentReward);
        Assert.Empty(response.GimmickSequenceClearReward);
    }

    [Fact]
    public async Task UpdateGimmickProgress_ExistingRecords_UpdatesWithoutDuplicating()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGimmick.Add(new EntityIUserGimmick
        {
            UserId = UserId,
            GimmickSequenceScheduleId = 10,
            GimmickSequenceId = 20,
            GimmickId = 30,
            StartDatetime = 1
        });
        userDb.EntityIUserGimmickOrnamentProgress.Add(new EntityIUserGimmickOrnamentProgress
        {
            UserId = UserId,
            GimmickSequenceScheduleId = 10,
            GimmickSequenceId = 20,
            GimmickId = 30,
            GimmickOrnamentIndex = 2,
            ProgressValueBit = 1,
            BaseDatetime = 1
        });

        GimmickServiceImpl service = CreateService(CreateMasterDb(), userDb);

        await service.UpdateGimmickProgress(
            new UpdateGimmickProgressRequest
            {
                GimmickSequenceScheduleId = 10,
                GimmickSequenceId = 20,
                GimmickId = 30,
                GimmickOrnamentIndex = 2,
                ProgressValueBit = 15
            },
            FakeServerCallContext.For(UserId));

        EntityIUserGimmick gimmick = Assert.Single(userDb.EntityIUserGimmick);
        Assert.True(gimmick.StartDatetime > 1);

        EntityIUserGimmickOrnamentProgress ornament = Assert.Single(userDb.EntityIUserGimmickOrnamentProgress);
        Assert.Equal(15, ornament.ProgressValueBit);
        Assert.True(ornament.BaseDatetime > 1);
    }

    [Fact]
    public async Task UpdateGimmickProgress_ReportGimmick_ClearsSequenceAndGrantsSequenceRewards()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureSequence(masterDb, sequenceId: 20, gimmickGroupId: 300, rewardGroupId: 400);
        AddGimmickToSequence(masterDb, gimmickGroupId: 300, gimmickId: 30, gimmickType: GimmickType.REPORT);
        AddSequenceReward(masterDb, rewardGroupId: 400, groupIndex: 1, possessionType: PossessionType.MATERIAL, possessionId: 500, count: 3);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        GimmickServiceImpl service = CreateService(masterDb, userDb);

        UpdateGimmickProgressResponse response = await service.UpdateGimmickProgress(
            new UpdateGimmickProgressRequest
            {
                GimmickSequenceScheduleId = 10,
                GimmickSequenceId = 20,
                GimmickId = 30,
                GimmickOrnamentIndex = 1,
                ProgressValueBit = 1
            },
            FakeServerCallContext.For(UserId));

        EntityIUserGimmick gimmick = Assert.Single(userDb.EntityIUserGimmick);
        Assert.True(gimmick.IsGimmickCleared);

        EntityIUserGimmickSequence sequence = Assert.Single(userDb.EntityIUserGimmickSequence);
        Assert.True(sequence.IsGimmickSequenceCleared);
        Assert.True(sequence.ClearDatetime > 0);

        EntityIUserMaterial material = Assert.Single(userDb.EntityIUserMaterial);
        Assert.Equal(500, material.MaterialId);
        Assert.Equal(3, material.Count);

        Assert.True(response.IsSequenceCleared);
        GimmickReward clearReward = Assert.Single(response.GimmickSequenceClearReward);
        Assert.Equal((int)PossessionType.MATERIAL, clearReward.PossessionType);
        Assert.Equal(500, clearReward.PossessionId);
        Assert.Equal(3, clearReward.Count);
    }

    [Fact]
    public async Task UpdateGimmickProgress_CageMemory_GrantsImportantItemWithoutClearingSequence()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureSequence(masterDb, sequenceId: 20, gimmickGroupId: 300, rewardGroupId: 0);
        AddGimmickToSequence(masterDb, gimmickGroupId: 300, gimmickId: 30, gimmickType: GimmickType.CAGE_MEMORY, gimmickOrnamentGroupId: 700);
        masterDb.EntityMGimmickOrnament.Add(new EntityMGimmickOrnament
        {
            GimmickOrnamentGroupId = 700,
            GimmickOrnamentIndex = 1,
            ChapterId = 80
        });
        masterDb.EntityMMainQuestChapter.Add(new EntityMMainQuestChapter
        {
            MainQuestChapterId = 80,
            MainQuestRouteId = 90
        });
        masterDb.EntityMMainQuestRoute.Add(new EntityMMainQuestRoute
        {
            MainQuestRouteId = 90,
            MainQuestSeasonId = 100
        });
        masterDb.EntityMCageMemory.Add(new EntityMCageMemory
        {
            CageMemoryId = 5000,
            MainQuestSeasonId = 100
        });
        masterDb.EntityMImportantItem.Add(new EntityMImportantItem
        {
            ImportantItemId = 6000,
            CageMemoryId = 5000,
            ImportantItemType = 4
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        GimmickServiceImpl service = CreateService(masterDb, userDb);

        UpdateGimmickProgressResponse response = await service.UpdateGimmickProgress(
            new UpdateGimmickProgressRequest
            {
                GimmickSequenceScheduleId = 10,
                GimmickSequenceId = 20,
                GimmickId = 30,
                GimmickOrnamentIndex = 1,
                ProgressValueBit = 1
            },
            FakeServerCallContext.For(UserId));

        EntityIUserImportantItem item = Assert.Single(userDb.EntityIUserImportantItem);
        Assert.Equal(6000, item.ImportantItemId);
        Assert.Equal(1, item.Count);

        Assert.False(Assert.Single(userDb.EntityIUserGimmick).IsGimmickCleared);
        Assert.Empty(userDb.EntityIUserGimmickSequence);
        Assert.False(response.IsSequenceCleared);
        GimmickReward ornamentReward = Assert.Single(response.GimmickOrnamentReward);
        Assert.Equal((int)PossessionType.IMPORTANT_ITEM, ornamentReward.PossessionType);
        Assert.Equal(6000, ornamentReward.PossessionId);
        Assert.Equal(1, ornamentReward.Count);
    }

    [Fact]
    public async Task InitSequenceSchedule_ActivatesOnlyCurrentSchedulesWithSatisfiedQuestConditions()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();

        ConfigureQuestClearCondition(masterDb, evaluateConditionId: 100, valueGroupId: 10, requiredQuestId: 5001);
        ConfigureQuestClearCondition(masterDb, evaluateConditionId: 101, valueGroupId: 11, requiredQuestId: 5002);
        ConfigureSequence(masterDb, sequenceId: 601, gimmickGroupId: 700, rewardGroupId: 0);
        AddGimmickToSequence(masterDb, gimmickGroupId: 700, gimmickId: 701, gimmickType: GimmickType.REPORT);
        // A hidden-story report is gated on its release quest like any other schedule (the real save holds exactly the
        // report schedules whose release quest it cleared).
        ConfigureQuestClearCondition(masterDb, evaluateConditionId: 102, valueGroupId: 12, requiredQuestId: 5003);

        AddSchedule(masterDb, scheduleId: 1, firstSequenceId: 101, startDatetime: nowMs - 1000, endDatetime: nowMs + 1000);
        AddSchedule(masterDb, scheduleId: 2, firstSequenceId: 201, startDatetime: nowMs - 1000, endDatetime: nowMs + 1000, releaseEvaluateConditionId: 100);
        AddSchedule(masterDb, scheduleId: 3, firstSequenceId: 301, startDatetime: nowMs - 1000, endDatetime: nowMs + 1000, releaseEvaluateConditionId: 101);
        AddSchedule(masterDb, scheduleId: 4, firstSequenceId: 401, startDatetime: nowMs + 1000, endDatetime: nowMs + 5000);
        AddSchedule(masterDb, scheduleId: 5, firstSequenceId: 501, startDatetime: nowMs - 5000, endDatetime: nowMs - 1000);
        AddSchedule(masterDb, scheduleId: 6, firstSequenceId: 601, startDatetime: nowMs - 1000, endDatetime: nowMs + 1000, releaseEvaluateConditionId: 102);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = 5001,
            QuestStateType = (int)QuestStateType.CLEARED,
            ClearCount = 1
        });

        GimmickServiceImpl service = CreateService(masterDb, userDb);

        await service.InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));

        // Schedule 5 has ended: the client wouldn't show it.
        Assert.Equal(2, userDb.EntityIUserGimmickSequence.Count);
        Assert.Contains(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == 1 && s.GimmickSequenceId == 101);
        Assert.Contains(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == 2 && s.GimmickSequenceId == 201);
        Assert.DoesNotContain(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == 5);
        Assert.DoesNotContain(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == 6);
        Assert.DoesNotContain(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == 3);
        Assert.DoesNotContain(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == 4);
    }

    [Fact]
    public async Task InitSequenceSchedule_ExistingSequence_DoesNotDuplicateRecord()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddSchedule(masterDb, scheduleId: 7, firstSequenceId: 701, startDatetime: nowMs - 1000, endDatetime: nowMs + 1000);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGimmickSequence.Add(new EntityIUserGimmickSequence
        {
            UserId = UserId,
            GimmickSequenceScheduleId = 7,
            GimmickSequenceId = 701,
            IsGimmickSequenceCleared = true,
            ClearDatetime = 2468
        });

        GimmickServiceImpl service = CreateService(masterDb, userDb);

        await service.InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));

        EntityIUserGimmickSequence sequence = Assert.Single(userDb.EntityIUserGimmickSequence);
        Assert.True(sequence.IsGimmickSequenceCleared);
        Assert.Equal(2468, sequence.ClearDatetime);
    }

    [Fact]
    public async Task InitSequenceSchedule_DropsUnfinishedSequencesOfEndedSchedules_KeepsClearedOnes()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();

        AddSchedule(masterDb, scheduleId: 10, firstSequenceId: 1001, startDatetime: nowMs - 2000, endDatetime: nowMs - 1000);
        AddSchedule(masterDb, scheduleId: 11, firstSequenceId: 1001, startDatetime: nowMs - 3000, endDatetime: nowMs + 1000);
        AddSchedule(masterDb, scheduleId: 12, firstSequenceId: 1201, startDatetime: nowMs - 2000, endDatetime: nowMs + 1000);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGimmickSequence.AddRange(
        [
            new EntityIUserGimmickSequence
            {
                UserId = UserId,
                GimmickSequenceScheduleId = 99,
                GimmickSequenceId = 999,
                IsGimmickSequenceCleared = false
            },
            new EntityIUserGimmickSequence
            {
                UserId = UserId,
                GimmickSequenceScheduleId = 98,
                GimmickSequenceId = 998,
                IsGimmickSequenceCleared = true,
                ClearDatetime = 1234
            }
        ]);

        GimmickServiceImpl service = CreateService(masterDb, userDb);

        await service.InitSequenceSchedule(new Empty(), FakeServerCallContext.For(UserId));

        Assert.DoesNotContain(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == 99);
        Assert.Contains(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == 98 && s.GimmickSequenceId == 998);
        Assert.Contains(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == 11 && s.GimmickSequenceId == 1001);
        Assert.DoesNotContain(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == 10);
        Assert.Contains(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == 12 && s.GimmickSequenceId == 1201);
    }

    [Fact]
    public async Task Unlock_MixedExistingAndDuplicateKeys_SetsUnlockedWithoutDuplicating()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGimmickUnlock.Add(new EntityIUserGimmickUnlock
        {
            UserId = UserId,
            GimmickSequenceScheduleId = 10,
            GimmickSequenceId = 20,
            GimmickId = 30,
            IsUnlocked = false
        });

        GimmickServiceImpl service = CreateService(CreateMasterDb(), userDb);

        await service.Unlock(
            new UnlockRequest
            {
                GimmickKey =
                {
                    new GimmickKey
                    {
                        GimmickSequenceScheduleId = 10,
                        GimmickSequenceId = 20,
                        GimmickId = 30
                    },
                    new GimmickKey
                    {
                        GimmickSequenceScheduleId = 10,
                        GimmickSequenceId = 20,
                        GimmickId = 30
                    },
                    new GimmickKey
                    {
                        GimmickSequenceScheduleId = 11,
                        GimmickSequenceId = 21,
                        GimmickId = 31
                    }
                }
            },
            FakeServerCallContext.For(UserId));

        Assert.Equal(2, userDb.EntityIUserGimmickUnlock.Count);
        Assert.Contains(userDb.EntityIUserGimmickUnlock, u =>
            u.GimmickSequenceScheduleId == 10 &&
            u.GimmickSequenceId == 20 &&
            u.GimmickId == 30 &&
            u.IsUnlocked);
        Assert.Contains(userDb.EntityIUserGimmickUnlock, u =>
            u.GimmickSequenceScheduleId == 11 &&
            u.GimmickSequenceId == 21 &&
            u.GimmickId == 31 &&
            u.IsUnlocked);
    }

    private static GimmickServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return new GimmickServiceImpl(store, masterDb, Config, TestServices.Missions(masterDb), TimeProvider.System);
    }

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static void AddSchedule(
        DarkMasterMemoryDatabase masterDb,
        int scheduleId,
        int firstSequenceId,
        long startDatetime,
        long endDatetime,
        int releaseEvaluateConditionId = 0)
    {
        masterDb.EntityMGimmickSequenceSchedule.Add(new EntityMGimmickSequenceSchedule
        {
            GimmickSequenceScheduleId = scheduleId,
            FirstGimmickSequenceId = firstSequenceId,
            StartDatetime = startDatetime,
            EndDatetime = endDatetime,
            ReleaseEvaluateConditionId = releaseEvaluateConditionId
        });
    }

    private static void ConfigureQuestClearCondition(
        DarkMasterMemoryDatabase masterDb,
        int evaluateConditionId,
        int valueGroupId,
        int requiredQuestId)
    {
        masterDb.EntityMEvaluateCondition.Add(new EntityMEvaluateCondition
        {
            EvaluateConditionId = evaluateConditionId,
            EvaluateConditionFunctionType = EvaluateConditionFunctionType.QUEST_CLEAR,
            EvaluateConditionEvaluateType = EvaluateConditionEvaluateType.ID_CONTAIN,
            EvaluateConditionValueGroupId = valueGroupId
        });
        masterDb.EntityMEvaluateConditionValueGroup.Add(new EntityMEvaluateConditionValueGroup
        {
            EvaluateConditionValueGroupId = valueGroupId,
            GroupIndex = 1,
            Value = requiredQuestId
        });
    }

    private static void ConfigureSequence(
        DarkMasterMemoryDatabase masterDb,
        int sequenceId,
        int gimmickGroupId,
        int rewardGroupId)
    {
        masterDb.EntityMGimmickSequence.Add(new EntityMGimmickSequence
        {
            GimmickSequenceId = sequenceId,
            GimmickGroupId = gimmickGroupId,
            GimmickSequenceRewardGroupId = rewardGroupId
        });
    }

    private static void AddGimmickToSequence(
        DarkMasterMemoryDatabase masterDb,
        int gimmickGroupId,
        int gimmickId,
        GimmickType gimmickType,
        int gimmickOrnamentGroupId = 0)
    {
        masterDb.EntityMGimmickGroup.Add(new EntityMGimmickGroup
        {
            GimmickGroupId = gimmickGroupId,
            GroupIndex = 1,
            GimmickId = gimmickId
        });
        masterDb.EntityMGimmick.Add(new EntityMGimmick
        {
            GimmickId = gimmickId,
            GimmickType = gimmickType,
            GimmickOrnamentGroupId = gimmickOrnamentGroupId
        });
    }

    private static void AddSequenceReward(
        DarkMasterMemoryDatabase masterDb,
        int rewardGroupId,
        int groupIndex,
        PossessionType possessionType,
        int possessionId,
        int count)
    {
        masterDb.EntityMGimmickSequenceRewardGroup.Add(new EntityMGimmickSequenceRewardGroup
        {
            GimmickSequenceRewardGroupId = rewardGroupId,
            GroupIndex = groupIndex,
            PossessionType = possessionType,
            PossessionId = possessionId,
            Count = count
        });
    }
}
