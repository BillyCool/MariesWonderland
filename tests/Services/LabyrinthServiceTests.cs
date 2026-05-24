using MariesWonderland.Proto.Data;
using MariesWonderland.Proto.Labyrinth;
using MariesWonderland.Tests.Infrastructure;
using LabyrinthServiceImpl = MariesWonderland.Services.LabyrinthService;

namespace MariesWonderland.Tests.Services;

public class LabyrinthServiceTests
{
    [Fact]
    public async Task UpdateSeasonData_ReturnsEmptyResponse()
    {
        LabyrinthServiceImpl service = CreateService();

        UpdateSeasonDataResponse response = await service.UpdateSeasonData(
            new UpdateSeasonDataRequest { EventQuestChapterId = 123 },
            FakeServerCallContext.For(userId: 1));

        Assert.NotNull(response);
        Assert.Empty(response.SeasonResult);
        Assert.Empty(response.DiffUserData);
    }

    [Fact]
    public async Task UpdateSeasonData_EachCallReturnsFreshCollections()
    {
        LabyrinthServiceImpl service = CreateService();

        UpdateSeasonDataResponse firstResponse = await service.UpdateSeasonData(
            new UpdateSeasonDataRequest { EventQuestChapterId = 1 },
            FakeServerCallContext.For(userId: 1));
        firstResponse.SeasonResult.Add(new LabyrinthSeasonResult { EventQuestChapterId = 99 });
        firstResponse.DiffUserData.Add("patched", new DiffData());

        UpdateSeasonDataResponse secondResponse = await service.UpdateSeasonData(
            new UpdateSeasonDataRequest { EventQuestChapterId = 2 },
            FakeServerCallContext.For(userId: 999));

        Assert.NotSame(firstResponse, secondResponse);
        Assert.Empty(secondResponse.SeasonResult);
        Assert.Empty(secondResponse.DiffUserData);
    }

    [Fact]
    public async Task ReceiveStageClearReward_ReturnsEmptyResponse()
    {
        LabyrinthServiceImpl service = CreateService();

        ReceiveStageClearRewardResponse response = await service.ReceiveStageClearReward(
            new ReceiveStageClearRewardRequest
            {
                EventQuestChapterId = 123,
                StageOrder = 4
            },
            FakeServerCallContext.For(userId: 1));

        Assert.NotNull(response);
        Assert.Empty(response.DiffUserData);
    }

    [Fact]
    public async Task ReceiveStageClearReward_EachCallReturnsFreshDiffUserData()
    {
        LabyrinthServiceImpl service = CreateService();

        ReceiveStageClearRewardResponse firstResponse = await service.ReceiveStageClearReward(
            new ReceiveStageClearRewardRequest { EventQuestChapterId = 1, StageOrder = 1 },
            FakeServerCallContext.For(userId: 1));
        firstResponse.DiffUserData.Add("patched", new DiffData());

        ReceiveStageClearRewardResponse secondResponse = await service.ReceiveStageClearReward(
            new ReceiveStageClearRewardRequest { EventQuestChapterId = 2, StageOrder = 2 },
            FakeServerCallContext.For(userId: 999));

        Assert.NotSame(firstResponse, secondResponse);
        Assert.Empty(secondResponse.DiffUserData);
    }

    [Fact]
    public async Task ReceiveStageAccumulationReward_ReturnsEmptyResponse()
    {
        LabyrinthServiceImpl service = CreateService();

        ReceiveStageAccumulationRewardResponse response = await service.ReceiveStageAccumulationReward(
            new ReceiveStageAccumulationRewardRequest
            {
                EventQuestChapterId = 123,
                StageOrder = 4,
                QuestMissionClearCount = 7
            },
            FakeServerCallContext.For(userId: 1));

        Assert.NotNull(response);
        Assert.Empty(response.DiffUserData);
    }

    [Fact]
    public async Task ReceiveStageAccumulationReward_EachCallReturnsFreshDiffUserData()
    {
        LabyrinthServiceImpl service = CreateService();

        ReceiveStageAccumulationRewardResponse firstResponse = await service.ReceiveStageAccumulationReward(
            new ReceiveStageAccumulationRewardRequest
            {
                EventQuestChapterId = 1,
                StageOrder = 1,
                QuestMissionClearCount = 1
            },
            FakeServerCallContext.For(userId: 1));
        firstResponse.DiffUserData.Add("patched", new DiffData());

        ReceiveStageAccumulationRewardResponse secondResponse = await service.ReceiveStageAccumulationReward(
            new ReceiveStageAccumulationRewardRequest
            {
                EventQuestChapterId = 2,
                StageOrder = 2,
                QuestMissionClearCount = 2
            },
            FakeServerCallContext.For(userId: 999));

        Assert.NotSame(firstResponse, secondResponse);
        Assert.Empty(secondResponse.DiffUserData);
    }

    private static LabyrinthServiceImpl CreateService() => new();
}
