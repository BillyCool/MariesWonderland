using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
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
        GimmickServiceImpl service = new(CreateStore(UserId, userDb), MasterDb, new FakeTimeProvider(Now));

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
        GimmickServiceImpl service = new(CreateStore(UserId, userDb), MasterDb, new FakeTimeProvider(Now));
        UpdateGimmickProgressRequest request = new() { GimmickSequenceScheduleId = 1, GimmickSequenceId = sequence.GimmickSequenceId, GimmickId = report.GimmickId };

        await service.UpdateGimmickProgress(request, ContextFor(UserId));
        Assert.False(userDb.EntityIUserGimmick.Single().IsGimmickCleared);

        userDb.EntityIUserMission.Add(new EntityIUserMission { UserId = UserId, MissionId = 500042, MissionProgressStatusType = MissionProgressStatusType.CLEAR });
        await service.UpdateGimmickProgress(request, ContextFor(UserId));
        Assert.True(userDb.EntityIUserGimmick.Single().IsGimmickCleared);
    }
}
