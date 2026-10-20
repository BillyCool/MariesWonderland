using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Quest;
using MariesWonderland.Quests;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using QuestService = MariesWonderland.Services.QuestService;

namespace MariesWonderland.Tests.Quests;

public class MainQuestFlowTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private (QuestService Service, DarkUserMemoryDatabase UserDb, long UserId) CreateService()
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("main-quest-flow-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        QuestCampaignHelper campaigns = new(MasterDb);
        QuestService service = new(store, MasterDb, new QuestEngine(MasterDb, GameConfig, campaigns, Options.Create(new DevModeOptions())), campaigns, TimeProvider.System);
        return (service, userDb, userId);
    }

    private EntityMQuest FindPlayableMainQuest()
    {
        HashSet<int> mainQuestIds = [.. MasterDb.EntityMMainQuestSequence.Select(s => s.QuestId)];
        return MasterDb.EntityMQuest
            .Where(q => mainQuestIds.Contains(q.QuestId) && !q.IsRunInTheBackground && q.IsCountedAsQuest)
            .OrderBy(q => q.QuestId)
            .First();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MenuPick_LeavesStoryPositionAndRestoresContext(bool isRetired)
    {
        (QuestService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        EntityMQuest quest = FindPlayableMainQuest();

        EntityIUserMainQuestMainFlowStatus mainFlow = userDb.EntityIUserMainQuestMainFlowStatus.GetOrCreate(userId);
        mainFlow.CurrentMainQuestRouteId = 3;
        mainFlow.CurrentQuestSceneId = 123456;
        mainFlow.HeadQuestSceneId = 123456;
        userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId).CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;
        userDb.EntityIUserPortalCageStatus.GetOrCreate(userId).IsCurrentProgress = true;
        EntityIUserSideStoryQuestSceneProgressStatus sideStory = userDb.EntityIUserSideStoryQuestSceneProgressStatus.GetOrCreate(userId);
        sideStory.CurrentSideStoryQuestId = 7;
        sideStory.CurrentSideStoryQuestSceneId = 70;

        await service.StartMainQuest(new StartMainQuestRequest { QuestId = quest.QuestId, IsMainFlow = false, UserDeckNumber = 1 }, ContextFor(userId));

        EntityIUserMainQuestProgressStatus progress = userDb.EntityIUserMainQuestProgressStatus.GetOrCreate(userId);
        Assert.NotEqual(0, progress.CurrentQuestSceneId);
        Assert.False(userDb.EntityIUserPortalCageStatus[0].IsCurrentProgress);
        Assert.Equal(0, sideStory.CurrentSideStoryQuestId);

        await service.FinishMainQuest(new FinishMainQuestRequest { QuestId = quest.QuestId, IsMainFlow = false, IsRetired = isRetired }, ContextFor(userId));

        Assert.Equal(3, mainFlow.CurrentMainQuestRouteId);
        Assert.Equal(123456, mainFlow.CurrentQuestSceneId);
        Assert.Equal(123456, mainFlow.HeadQuestSceneId);
        Assert.Equal(QuestFlowType.MAIN_FLOW, userDb.EntityIUserMainQuestFlowStatus[0].CurrentQuestFlowType);
        Assert.True(userDb.EntityIUserPortalCageStatus[0].IsCurrentProgress);
        Assert.Equal(7, sideStory.CurrentSideStoryQuestId);
        Assert.Equal(70, sideStory.CurrentSideStoryQuestSceneId);
        Assert.Empty(userDb.EntitySQuestSession);

        EntityIUserQuest userQuest = userDb.EntityIUserQuest.Single(q => q.QuestId == quest.QuestId);
        Assert.Equal(isRetired ? (int)QuestStateType.ACTIVE : (int)QuestStateType.CLEARED, userQuest.QuestStateType);
    }

    [Fact]
    public async Task MainFlowSceneProgress_BehindTheHead_LeavesStoryPosition()
    {
        (QuestService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        HashSet<int> mainQuestIds = [.. MasterDb.EntityMMainQuestSequence.Select(s => s.QuestId)];
        List<EntityMQuestScene> scenes = [.. MasterDb.EntityMQuestScene
            .Where(s => mainQuestIds.Contains(s.QuestId))
            .OrderBy(s => s.SortOrder)];
        EntityMQuestScene early = scenes[0];
        EntityMQuestScene late = scenes[^1];

        await service.UpdateMainFlowSceneProgress(new UpdateMainFlowSceneProgressRequest { QuestSceneId = late.QuestSceneId }, ContextFor(userId));
        EntityIUserMainQuestMainFlowStatus mainFlow = userDb.EntityIUserMainQuestMainFlowStatus.GetOrCreate(userId);
        int routeAfterLate = mainFlow.CurrentMainQuestRouteId;

        await service.UpdateMainFlowSceneProgress(new UpdateMainFlowSceneProgressRequest { QuestSceneId = early.QuestSceneId }, ContextFor(userId));

        Assert.Equal(late.QuestSceneId, mainFlow.CurrentQuestSceneId);
        Assert.Equal(late.QuestSceneId, mainFlow.HeadQuestSceneId);
        Assert.Equal(routeAfterLate, mainFlow.CurrentMainQuestRouteId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SceneProgress_IntoAnotherQuest_ClearsAPassedBackgroundQuestOnly(bool previousIsPlayable)
    {
        (QuestService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        HashSet<int> sceneQuestIds = [.. MasterDb.EntityMQuestScene.Select(s => s.QuestId)];
        EntityMQuest next = FindPlayableMainQuest();
        EntityMQuest previous = MasterDb.EntityMQuest
            .Where(q => q.QuestId != next.QuestId && sceneQuestIds.Contains(q.QuestId) && QuestEngine.IsPlayable(q) == previousIsPlayable && (previousIsPlayable || q.IsRunInTheBackground))
            .OrderBy(q => q.QuestId)
            .First();
        EntityMQuestScene previousScene = MasterDb.EntityMQuestScene.First(s => s.QuestId == previous.QuestId);
        EntityMQuestScene nextScene = MasterDb.EntityMQuestScene.First(s => s.QuestId == next.QuestId);

        userDb.EntityIUserMainQuestProgressStatus.GetOrCreate(userId).CurrentQuestSceneId = previousScene.QuestSceneId;

        await service.UpdateMainQuestSceneProgress(new UpdateMainQuestSceneProgressRequest { QuestSceneId = nextScene.QuestSceneId }, ContextFor(userId));

        EntityIUserQuest? previousUserQuest = userDb.EntityIUserQuest.FirstOrDefault(q => q.QuestId == previous.QuestId);
        if (previousIsPlayable)
        {
            Assert.Null(previousUserQuest);
        }
        else
        {
            Assert.NotNull(previousUserQuest);
            Assert.Equal((int)QuestStateType.CLEARED, previousUserQuest.QuestStateType);
            Assert.Equal(1, previousUserQuest.ClearCount);
        }
    }

    [Fact]
    public async Task SetRoute_RecordsTheFirstRoutePerSeason()
    {
        (QuestService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();

        await service.SetRoute(new SetRouteRequest { MainQuestRouteId = 1 }, ContextFor(userId));
        await service.SetRoute(new SetRouteRequest { MainQuestRouteId = 2 }, ContextFor(userId));
        await service.SetRoute(new SetRouteRequest { MainQuestRouteId = 3 }, ContextFor(userId));

        Assert.Equal(2, userDb.EntityIUserMainQuestSeasonRoute.Count);
        Assert.Equal(1, userDb.EntityIUserMainQuestSeasonRoute.Single(r => r.MainQuestSeasonId == 1).MainQuestRouteId);
        Assert.Equal(2, userDb.EntityIUserMainQuestSeasonRoute.Single(r => r.MainQuestSeasonId == 2).MainQuestRouteId);
        Assert.Equal(3, userDb.EntityIUserMainQuestMainFlowStatus[0].CurrentMainQuestRouteId);
    }

    [Theory]
    [InlineData(2, QuestFlowType.REPLAY_FLOW)]
    [InlineData(3, QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW)]
    public async Task ReplayStart_OnAnotherRouteOfTheSeason_IsAnotherRouteReplay(int questRouteId, QuestFlowType expected)
    {
        (QuestService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        await service.SetRoute(new SetRouteRequest { MainQuestRouteId = 2 }, ContextFor(userId));

        int sequenceGroupId = MasterDb.EntityMMainQuestChapter.First(c => c.MainQuestRouteId == questRouteId).MainQuestSequenceGroupId;
        HashSet<int> sequenceIds = [.. MasterDb.EntityMMainQuestSequenceGroup
            .Where(g => g.MainQuestSequenceGroupId == sequenceGroupId)
            .Select(g => g.MainQuestSequenceId)];
        int questId = MasterDb.EntityMMainQuestSequence
            .Where(s => sequenceIds.Contains(s.MainQuestSequenceId))
            .Select(s => s.QuestId)
            .First(id => MasterDb.EntityMQuestScene.Any(s => s.QuestId == id));

        await service.StartMainQuest(new StartMainQuestRequest { QuestId = questId, IsReplayFlow = true, UserDeckNumber = 1 }, ContextFor(userId));

        Assert.Equal(expected, userDb.EntityIUserMainQuestFlowStatus[0].CurrentQuestFlowType);
    }
}
