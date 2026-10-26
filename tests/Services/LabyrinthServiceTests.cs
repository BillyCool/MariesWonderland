using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Labyrinth;
using MariesWonderland.Proto.Reward;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Time.Testing;
using LabyrinthServiceImpl = MariesWonderland.Services.LabyrinthService;
using RewardServiceImpl = MariesWonderland.Services.RewardService;

namespace MariesWonderland.Tests.Services;

/// <summary>
/// Fate Board: a board of four Normal quests (1001-1004) in two stages, two seasons back to back.
/// </summary>
public class LabyrinthServiceTests
{
    private const long UserId = 1L;
    private const int ChapterId = 100;
    private const int SequenceId = 900;
    private const int CompleteMissionId = 99;
    private static readonly DateTimeOffset SeasonOneStart = new(2030, 1, 1, 2, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset SeasonTwoStart = SeasonOneStart.AddDays(14);

    private readonly DarkMasterMemoryDatabase _masterDb = CreateMasterDb();
    private readonly DarkUserMemoryDatabase _userDb = new() { UserId = UserId };
    private readonly FakeTimeProvider _time = new(SeasonOneStart.AddDays(1));
    private readonly UserDataStore _store;

    public LabyrinthServiceTests()
    {
        _store = new UserDataStore(_time);
        _store.Set(UserId, _userDb);
    }

    [Fact]
    public async Task UpdateSeasonData_JoinsTheCurrentSeasonWithNoResult()
    {
        UpdateSeasonDataResponse response = await OpenBoard();

        Assert.Empty(response.SeasonResult);
        EntityIUserEventQuestLabyrinthSeason season = Assert.Single(_userDb.EntityIUserEventQuestLabyrinthSeason);
        Assert.Equal(1, season.LastJoinSeasonNumber);
        Assert.Equal(0, season.LastSeasonRewardReceivedSeasonNumber);
    }

    [Fact]
    public async Task UpdateSeasonData_InTheNextSeasonPaysTheFurthestClearedHeadQuestOnce()
    {
        await OpenBoard();
        ClearQuests(1001, 1002, 1003);
        _time.SetUtcNow(SeasonTwoStart.AddDays(1));

        UpdateSeasonDataResponse first = await OpenBoard();
        UpdateSeasonDataResponse second = await OpenBoard();

        LabyrinthSeasonResult result = Assert.Single(first.SeasonResult);
        Assert.Equal(ChapterId, result.EventQuestChapterId);
        Assert.Equal(1003, result.HeadQuestId);
        Assert.Equal(2, result.HeadStageOrder);
        LabyrinthReward reward = Assert.Single(result.SeasonReward);
        Assert.Equal((int)PossessionType.MATERIAL, reward.PossessionType);
        Assert.Equal(502, reward.PossessionId);
        Assert.Equal(2, reward.Count);
        Assert.Empty(second.SeasonResult);
        Assert.Equal(2, Assert.Single(_userDb.EntityIUserEventQuestLabyrinthSeason).LastJoinSeasonNumber);
        Assert.Equal(1, Assert.Single(_userDb.EntityIUserEventQuestLabyrinthSeason).LastSeasonRewardReceivedSeasonNumber);
        Assert.Equal(2, Assert.Single(_userDb.EntityIUserMaterial).Count);
    }

    [Fact]
    public async Task UpdateSeasonData_PaysNothingWhileTheJoinedSeasonIsRunning()
    {
        await OpenBoard();
        ClearQuests(1001);

        UpdateSeasonDataResponse response = await OpenBoard();

        Assert.Empty(response.SeasonResult);
        Assert.Empty(_userDb.EntityIUserMaterial);
    }

    [Fact]
    public async Task UpdateSeasonData_JoinsNothingWhenSeasonsOverlap()
    {
        _masterDb.EntityMEventQuestLabyrinthSeason.Add(new EntityMEventQuestLabyrinthSeason
        {
            EventQuestChapterId = ChapterId,
            SeasonNumber = 3,
            StartDatetime = SeasonOneStart.ToUnixTimeMilliseconds(),
            EndDatetime = SeasonTwoStart.AddDays(30).ToUnixTimeMilliseconds(),
            SeasonRewardGroupId = 700
        });

        await OpenBoard();

        Assert.Empty(_userDb.EntityIUserEventQuestLabyrinthSeason);
    }

    [Fact]
    public async Task ReceiveLabyrinthSeasonReward_PaysEndedSeasonsOnTheHomeScreen()
    {
        await OpenBoard();
        ClearQuests(1001);
        _time.SetUtcNow(SeasonTwoStart.AddDays(1));
        RewardServiceImpl rewards = new(_store, _masterDb, _time);

        ReceiveLabyrinthSeasonRewardResponse first = await rewards.ReceiveLabyrinthSeasonReward(new Empty(), FakeServerCallContext.For(UserId));
        ReceiveLabyrinthSeasonRewardResponse second = await rewards.ReceiveLabyrinthSeasonReward(new Empty(), FakeServerCallContext.For(UserId));

        LabyrinthSeasonResult result = Assert.Single(first.SeasonResult);
        Assert.Equal(1001, result.HeadQuestId);
        Assert.Equal(1, result.HeadStageOrder);
        Assert.Empty(second.SeasonResult);
        Assert.Equal(501, Assert.Single(_userDb.EntityIUserMaterial).MaterialId);
    }

    [Fact]
    public async Task ReceiveLabyrinthSeasonReward_MarksASeasonWithNothingClearedAsPaid()
    {
        await OpenBoard();
        _time.SetUtcNow(SeasonTwoStart.AddDays(1));
        RewardServiceImpl rewards = new(_store, _masterDb, _time);

        ReceiveLabyrinthSeasonRewardResponse response = await rewards.ReceiveLabyrinthSeasonReward(new Empty(), FakeServerCallContext.For(UserId));

        Assert.Empty(response.SeasonResult);
        Assert.Equal(1, Assert.Single(_userDb.EntityIUserEventQuestLabyrinthSeason).LastSeasonRewardReceivedSeasonNumber);
    }

    [Fact]
    public async Task ReceiveStageClearReward_NeedsEveryStageQuestClearedThenPaysOnce()
    {
        LabyrinthServiceImpl service = CreateService();
        ReceiveStageClearRewardRequest request = new() { EventQuestChapterId = ChapterId, StageOrder = 1 };

        ClearQuests(1001);
        await service.ReceiveStageClearReward(request, FakeServerCallContext.For(UserId));
        Assert.Empty(_userDb.EntityIUserMaterial);

        ClearQuests(1002);
        await service.ReceiveStageClearReward(request, FakeServerCallContext.For(UserId));
        await service.ReceiveStageClearReward(request, FakeServerCallContext.For(UserId));

        Assert.True(Assert.Single(_userDb.EntityIUserEventQuestLabyrinthStage).IsReceivedStageClearReward);
        EntityIUserMaterial reward = Assert.Single(_userDb.EntityIUserMaterial);
        Assert.Equal(601, reward.MaterialId);
        Assert.Equal(3, reward.Count);
    }

    [Fact]
    public async Task ReceiveStageAccumulationReward_PaysOnlyTiersTheClearedMissionsReach()
    {
        LabyrinthServiceImpl service = CreateService();
        ClearMissions(1001, 1, 2, 3, CompleteMissionId);

        await service.ReceiveStageAccumulationReward(
            new ReceiveStageAccumulationRewardRequest { EventQuestChapterId = ChapterId, StageOrder = 1, QuestMissionClearCount = 5 },
            FakeServerCallContext.For(UserId));
        Assert.Equal(["602:1"], Materials());
        Assert.Equal(3, Assert.Single(_userDb.EntityIUserEventQuestLabyrinthStage).AccumulationRewardReceivedQuestMissionCount);

        ClearMissions(1002, 1, 2);
        await service.ReceiveStageAccumulationReward(
            new ReceiveStageAccumulationRewardRequest { EventQuestChapterId = ChapterId, StageOrder = 1, QuestMissionClearCount = 5 },
            FakeServerCallContext.For(UserId));
        await service.ReceiveStageAccumulationReward(
            new ReceiveStageAccumulationRewardRequest { EventQuestChapterId = ChapterId, StageOrder = 1, QuestMissionClearCount = 5 },
            FakeServerCallContext.For(UserId));

        Assert.Equal(["602:1", "603:2"], Materials());
        Assert.Equal(5, Assert.Single(_userDb.EntityIUserEventQuestLabyrinthStage).AccumulationRewardReceivedQuestMissionCount);
    }

    private Task<UpdateSeasonDataResponse> OpenBoard()
        => CreateService().UpdateSeasonData(new UpdateSeasonDataRequest { EventQuestChapterId = ChapterId }, FakeServerCallContext.For(UserId));

    private LabyrinthServiceImpl CreateService() => new(_store, _masterDb, _time);

    private void ClearQuests(params int[] questIds)
    {
        foreach (int questId in questIds)
        {
            _userDb.EntityIUserQuest.Add(new EntityIUserQuest { UserId = UserId, QuestId = questId, ClearCount = 1 });
        }
    }

    private void ClearMissions(int questId, params int[] missionIds)
    {
        foreach (int missionId in missionIds)
        {
            _userDb.EntityIUserQuestMission.Add(new EntityIUserQuestMission { UserId = UserId, QuestId = questId, QuestMissionId = missionId, IsClear = true });
        }
    }

    private string[] Materials()
        => [.. _userDb.EntityIUserMaterial.OrderBy(m => m.MaterialId).Select(m => $"{m.MaterialId}:{m.Count}")];

    private static DarkMasterMemoryDatabase CreateMasterDb()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMEventQuestChapter.Add(new EntityMEventQuestChapter { EventQuestChapterId = ChapterId, EventQuestType = EventQuestType.LABYRINTH, EventQuestSequenceGroupId = SequenceId });
        masterDb.EntityMEventQuestSequenceGroup.Add(new EntityMEventQuestSequenceGroup { EventQuestSequenceGroupId = SequenceId, DifficultyType = DifficultyType.NORMAL, EventQuestSequenceId = SequenceId });
        for (int i = 1; i <= 4; i++)
        {
            masterDb.EntityMEventQuestSequence.Add(new EntityMEventQuestSequence { EventQuestSequenceId = SequenceId, SortOrder = i, QuestId = 1000 + i });
        }

        masterDb.EntityMQuestMission.Add(new EntityMQuestMission { QuestMissionId = CompleteMissionId, QuestMissionConditionType = QuestMissionConditionType.COMPLETE });
        masterDb.EntityMEventQuestLabyrinthSeason.AddRange(
        [
            new EntityMEventQuestLabyrinthSeason
            {
                EventQuestChapterId = ChapterId,
                SeasonNumber = 1,
                StartDatetime = SeasonOneStart.ToUnixTimeMilliseconds(),
                EndDatetime = SeasonTwoStart.ToUnixTimeMilliseconds() - 1000,
                SeasonRewardGroupId = 700
            },
            new EntityMEventQuestLabyrinthSeason
            {
                EventQuestChapterId = ChapterId,
                SeasonNumber = 2,
                StartDatetime = SeasonTwoStart.ToUnixTimeMilliseconds(),
                EndDatetime = SeasonTwoStart.AddDays(14).ToUnixTimeMilliseconds() - 1000,
                SeasonRewardGroupId = 700
            }
        ]);
        masterDb.EntityMEventQuestLabyrinthSeasonRewardGroup.AddRange(
        [
            new EntityMEventQuestLabyrinthSeasonRewardGroup { EventQuestLabyrinthSeasonRewardGroupId = 700, HeadQuestId = 1001, EventQuestLabyrinthRewardGroupId = 701 },
            new EntityMEventQuestLabyrinthSeasonRewardGroup { EventQuestLabyrinthSeasonRewardGroupId = 700, HeadQuestId = 1003, EventQuestLabyrinthRewardGroupId = 702 },
            new EntityMEventQuestLabyrinthSeasonRewardGroup { EventQuestLabyrinthSeasonRewardGroupId = 700, HeadQuestId = 1004, EventQuestLabyrinthRewardGroupId = 702 }
        ]);
        masterDb.EntityMEventQuestLabyrinthStage.AddRange(
        [
            new EntityMEventQuestLabyrinthStage
            {
                EventQuestChapterId = ChapterId,
                StageOrder = 1,
                StartSequenceSortOrder = 1,
                EndSequenceSortOrder = 2,
                StageClearRewardGroupId = 703,
                StageAccumulationRewardGroupId = 704
            },
            new EntityMEventQuestLabyrinthStage
            {
                EventQuestChapterId = ChapterId,
                StageOrder = 2,
                StartSequenceSortOrder = 3,
                EndSequenceSortOrder = 4
            }
        ]);
        masterDb.EntityMEventQuestLabyrinthStageAccumulationRewardGroup.AddRange(
        [
            new EntityMEventQuestLabyrinthStageAccumulationRewardGroup { EventQuestLabyrinthStageAccumulationRewardGroupId = 704, QuestMissionClearCount = 3, EventQuestLabyrinthRewardGroupId = 705 },
            new EntityMEventQuestLabyrinthStageAccumulationRewardGroup { EventQuestLabyrinthStageAccumulationRewardGroupId = 704, QuestMissionClearCount = 5, EventQuestLabyrinthRewardGroupId = 706 }
        ]);
        masterDb.EntityMEventQuestLabyrinthRewardGroup.AddRange(
        [
            new EntityMEventQuestLabyrinthRewardGroup { EventQuestLabyrinthRewardGroupId = 701, SortOrder = 1, PossessionType = PossessionType.MATERIAL, PossessionId = 501, Count = 1 },
            new EntityMEventQuestLabyrinthRewardGroup { EventQuestLabyrinthRewardGroupId = 702, SortOrder = 1, PossessionType = PossessionType.MATERIAL, PossessionId = 502, Count = 2 },
            new EntityMEventQuestLabyrinthRewardGroup { EventQuestLabyrinthRewardGroupId = 703, SortOrder = 1, PossessionType = PossessionType.MATERIAL, PossessionId = 601, Count = 3 },
            new EntityMEventQuestLabyrinthRewardGroup { EventQuestLabyrinthRewardGroupId = 705, SortOrder = 1, PossessionType = PossessionType.MATERIAL, PossessionId = 602, Count = 1 },
            new EntityMEventQuestLabyrinthRewardGroup { EventQuestLabyrinthRewardGroupId = 706, SortOrder = 1, PossessionType = PossessionType.MATERIAL, PossessionId = 603, Count = 2 }
        ]);
        return masterDb;
    }
}
