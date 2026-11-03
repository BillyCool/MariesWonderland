using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Gimmick;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using GimmickServiceImpl = MariesWonderland.Services.GimmickService;

namespace MariesWonderland.Tests.Services;

/// <summary>
/// Hidden stories (report gimmicks) and cage memories: their schedules open once the release quest is cleared, and a
/// report clears once its prerequisites (account missions, quest missions) are met.
/// </summary>
public class HiddenStoryTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private const long UserId = 1L;
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 0, 0, 0, TimeSpan.Zero);

    private DarkUserMemoryDatabase LoadRealSave()
        => new UserDataSeeder(Options.Create(new ServerOptions { Data = new DataOptions { UserDataPath = "Data/UserData" } })).LoadFromFiles();

    private GimmickType? SequenceType(int gimmickSequenceId)
    {
        EntityMGimmickSequence? sequence = MasterDb.EntityMGimmickSequence.Find(gimmickSequenceId);
        int? gimmickId = MasterDb.EntityMGimmickGroup.Where(g => g.GimmickGroupId == sequence?.GimmickGroupId).OrderBy(g => g.GroupIndex).Select(g => (int?)g.GimmickId).FirstOrDefault();
        return gimmickId == null ? null : MasterDb.EntityMGimmick.Find(gimmickId.Value)?.GimmickType;
    }

    [Fact]
    public void Evaluator_AgreesWithTheRealSave()
    {
        DarkUserMemoryDatabase save = LoadRealSave();
        Assert.NotEmpty(save.EntityIUserGimmickSequence);

        // Every report the save cleared passes its prerequisites.
        List<EntityIUserGimmick> clearedReports = [.. save.EntityIUserGimmick.Where(g => g.IsGimmickCleared && MasterDb.EntityMGimmick.Find(g.GimmickId)?.GimmickType == GimmickType.REPORT)];
        Assert.NotEmpty(clearedReports);
        Assert.All(clearedReports, g => Assert.True(EvaluateConditionHelper.Evaluate(MasterDb.EntityMGimmick.Find(g.GimmickId)!.ClearEvaluateConditionId, save, MasterDb), $"gimmick {g.GimmickId}"));

        // The save holds exactly the report schedules whose release quest it cleared.
        HashSet<int> heldSchedules = [.. save.EntityIUserGimmickSequence.Select(s => s.GimmickSequenceScheduleId)];
        List<EntityMGimmickSequenceSchedule> reportSchedules = [.. MasterDb.EntityMGimmickSequenceSchedule.Where(s => SequenceType(s.FirstGimmickSequenceId) == GimmickType.REPORT)];
        Assert.All(reportSchedules, s => Assert.Equal(heldSchedules.Contains(s.GimmickSequenceScheduleId), EvaluateConditionHelper.Evaluate(s.ReleaseEvaluateConditionId, save, MasterDb)));
    }

    [Fact]
    public async Task InitSequenceSchedule_OpensAReportScheduleOnlyAfterItsReleaseQuest()
    {
        EntityMGimmickSequenceSchedule schedule = MasterDb.EntityMGimmickSequenceSchedule
            .Where(s => s.ReleaseEvaluateConditionId != 0 && SequenceType(s.FirstGimmickSequenceId) == GimmickType.REPORT)
            .OrderBy(s => s.GimmickSequenceScheduleId).First();
        int releaseQuestId = (int)MasterDb.EntityMEvaluateConditionValueGroup
            .Single(v => v.EvaluateConditionValueGroupId == MasterDb.EntityMEvaluateCondition.Find(schedule.ReleaseEvaluateConditionId)!.EvaluateConditionValueGroupId).Value;
        DarkUserMemoryDatabase userDb = CreateUserDb();
        GimmickServiceImpl service = new(CreateStore(UserId, userDb), MasterDb, GameConfig, new MissionTracker(MasterDb, new MissionCatalog(MasterDb, MissionFilterTable.Empty)), new FakeTimeProvider(Now));

        await service.InitSequenceSchedule(new Empty(), ContextFor(UserId));
        Assert.DoesNotContain(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == schedule.GimmickSequenceScheduleId);

        userDb.EntityIUserQuest.Add(new EntityIUserQuest { UserId = UserId, QuestId = releaseQuestId, QuestStateType = (int)QuestStateType.CLEARED, ClearCount = 1 });
        await service.InitSequenceSchedule(new Empty(), ContextFor(UserId));
        Assert.Contains(userDb.EntityIUserGimmickSequence, s => s.GimmickSequenceScheduleId == schedule.GimmickSequenceScheduleId);
    }

    [Fact]
    public async Task Report_ClearsOnlyOnceItsMissionIsCleared()
    {
        // Gimmick 91011006 clears with account mission 500042.
        EntityMGimmick report = MasterDb.EntityMGimmick.Find(91011006)!;
        int groupId = MasterDb.EntityMGimmickGroup.First(g => g.GimmickId == report.GimmickId).GimmickGroupId;
        EntityMGimmickSequence sequence = MasterDb.EntityMGimmickSequence.First(s => s.GimmickGroupId == groupId);
        DarkUserMemoryDatabase userDb = CreateUserDb();
        GimmickServiceImpl service = new(CreateStore(UserId, userDb), MasterDb, GameConfig, new MissionTracker(MasterDb, new MissionCatalog(MasterDb, MissionFilterTable.Empty)), new FakeTimeProvider(Now));
        UpdateGimmickProgressRequest request = new() { GimmickSequenceScheduleId = 1, GimmickSequenceId = sequence.GimmickSequenceId, GimmickId = report.GimmickId };

        await service.UpdateGimmickProgress(request, ContextFor(UserId));
        Assert.False(userDb.EntityIUserGimmick.Single().IsGimmickCleared);

        userDb.EntityIUserMission.Add(new EntityIUserMission { UserId = UserId, MissionId = 500042, MissionProgressStatusType = MissionProgressStatusType.CLEAR });
        await service.UpdateGimmickProgress(request, ContextFor(UserId));
        Assert.True(userDb.EntityIUserGimmick.Single().IsGimmickCleared);
    }

    [Fact(Skip = "The fickle black bird rotation is left out of the modded master file since 2026-11-03 (suspected cause of the map issue); see docs/dev-tools/master-data.md.")]
    public void FickleBlackBirds_KeepTheirLiveRotationUntil2041()
    {
        // The 2021-12 → 2024-04 rotation is replayed in the modded master file (7 times, 876 days apart).
        List<EntityMGimmickSequenceSchedule> fickle = [.. MasterDb.EntityMGimmickSequenceSchedule.Where(s => SequenceType(s.FirstGimmickSequenceId) == GimmickType.CAGE_TREASURE_HUNT)];
        foreach (DateTimeOffset day in new[] { Now, new DateTimeOffset(2033, 3, 1, 12, 0, 0, TimeSpan.Zero), new DateTimeOffset(2040, 12, 1, 12, 0, 0, TimeSpan.Zero) })
        {
            long ms = day.ToUnixTimeMilliseconds();
            int running = fickle.Count(s => s.StartDatetime <= ms && ms <= s.EndDatetime);
            Assert.InRange(running, 5, 12);
        }

        long after = new DateTimeOffset(2041, 3, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        Assert.DoesNotContain(fickle, s => s.EndDatetime > after);
    }

    [Fact]
    public void LostArchives_WaitForOneOfTheSunAndTheMoonIIIArchives()
    {
        // Archive gimmick 101001001 (chapter 2) has release condition 6001: any of the four archives of chapters 18/26.
        EntityMGimmick archive = MasterDb.EntityMGimmick.Find(101001001)!;
        Assert.Equal(6001, archive.ReleaseEvaluateConditionId);
        int sequenceId = MasterDb.EntityMGimmickSequence.First(s => MasterDb.EntityMGimmickGroup.Any(g => g.GimmickGroupId == s.GimmickGroupId && g.GimmickId == archive.GimmickId)).GimmickSequenceId;
        int scheduleId = MasterDb.EntityMGimmickSequenceSchedule.First(s => s.FirstGimmickSequenceId == sequenceId).GimmickSequenceScheduleId;
        DarkUserMemoryDatabase userDb = CreateUserDb();
        long nowMs = Now.ToUnixTimeMilliseconds();

        CageGimmickHelper.SeedSequenceGimmicks(MasterDb, userDb, scheduleId, sequenceId, nowMs);
        Assert.Empty(userDb.EntityIUserGimmick);

        // Read archive 102015015 (schedule and sequence 1001515, ornament 1).
        userDb.EntityIUserGimmickOrnamentProgress.Add(new EntityIUserGimmickOrnamentProgress { GimmickSequenceScheduleId = 1001515, GimmickSequenceId = 1001515, GimmickId = 102015015, GimmickOrnamentIndex = 1, ProgressValueBit = 1 });
        CageGimmickHelper.SeedSequenceGimmicks(MasterDb, userDb, scheduleId, sequenceId, nowMs);
        Assert.Contains(userDb.EntityIUserGimmick, g => g.GimmickId == archive.GimmickId);
    }
}
