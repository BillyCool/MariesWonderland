using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Quest;
using MariesWonderland.Tests.Infrastructure;
using QuestServiceImpl = MariesWonderland.Services.QuestService;

namespace MariesWonderland.Tests.Services;

public class QuestServiceTests
{
    private const long UserId = 1L;
    private const int PlayableQuestId = 101;
    private const int PlayableFirstSceneId = 1001;
    private const int PlayableLastSceneId = 1002;
    private const int PlayableRouteId = 2;
    private const int PlayableSeasonId = 7;
    private const int BackgroundQuestId = 201;
    private const int BackgroundSceneId = 2001;
    private const int BackgroundRouteId = 3;
    private const int BackgroundSeasonId = 9;

    [Fact]
    public async Task UpdateMainFlowSceneProgress_SyncsRouteSeasonAndSceneState()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        userDb.EntityIUserSideStoryQuestSceneProgressStatus.Add(new EntityIUserSideStoryQuestSceneProgressStatus
        {
            UserId = UserId,
            CurrentSideStoryQuestId = 99,
            CurrentSideStoryQuestSceneId = 999
        });
        userDb.EntityIUserPortalCageStatus.Add(new EntityIUserPortalCageStatus
        {
            UserId = UserId,
            IsCurrentProgress = true
        });

        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.UpdateMainFlowSceneProgress(
            new UpdateMainFlowSceneProgressRequest { QuestSceneId = PlayableFirstSceneId },
            FakeServerCallContext.For(UserId));

        EntityIUserMainQuestFlowStatus flowStatus = Assert.Single(userDb.EntityIUserMainQuestFlowStatus);
        Assert.Equal(QuestFlowType.MAIN_FLOW, flowStatus.CurrentQuestFlowType);

        EntityIUserMainQuestMainFlowStatus mainFlowStatus = Assert.Single(userDb.EntityIUserMainQuestMainFlowStatus);
        Assert.Equal(PlayableFirstSceneId, mainFlowStatus.CurrentQuestSceneId);
        Assert.Equal(PlayableFirstSceneId, mainFlowStatus.HeadQuestSceneId);
        Assert.Equal(PlayableRouteId, mainFlowStatus.CurrentMainQuestRouteId);
        Assert.False(mainFlowStatus.IsReachedLastQuestScene);

        EntityIUserMainQuestSeasonRoute seasonRoute = Assert.Single(userDb.EntityIUserMainQuestSeasonRoute);
        Assert.Equal(PlayableRouteId, seasonRoute.MainQuestRouteId);
        Assert.Equal(PlayableSeasonId, seasonRoute.MainQuestSeasonId);

        EntityIUserSideStoryQuestSceneProgressStatus sideStoryStatus = Assert.Single(userDb.EntityIUserSideStoryQuestSceneProgressStatus);
        Assert.Equal(0, sideStoryStatus.CurrentSideStoryQuestId);
        Assert.Equal(0, sideStoryStatus.CurrentSideStoryQuestSceneId);

        EntityIUserPortalCageStatus portalCageStatus = Assert.Single(userDb.EntityIUserPortalCageStatus);
        Assert.False(portalCageStatus.IsCurrentProgress);
    }

    [Fact]
    public async Task FinishMainQuest_MainFlowVictory_RepairsSceneRouteAndSeasonTogether()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = PlayableQuestId,
            QuestStateType = (int)QuestStateType.ACTIVE,
            ClearCount = 1
        });
        userDb.EntityIUserMainQuestFlowStatus.Add(new EntityIUserMainQuestFlowStatus
        {
            UserId = UserId,
            CurrentQuestFlowType = QuestFlowType.MAIN_FLOW
        });
        userDb.EntityIUserMainQuestProgressStatus.Add(new EntityIUserMainQuestProgressStatus
        {
            UserId = UserId,
            CurrentQuestSceneId = PlayableFirstSceneId,
            HeadQuestSceneId = PlayableFirstSceneId,
            CurrentQuestFlowType = QuestFlowType.SUB_FLOW
        });
        userDb.EntityIUserMainQuestMainFlowStatus.Add(new EntityIUserMainQuestMainFlowStatus
        {
            UserId = UserId
        });
        userDb.EntityIUserMainQuestSeasonRoute.Add(new EntityIUserMainQuestSeasonRoute
        {
            UserId = UserId,
            MainQuestRouteId = 1,
            MainQuestSeasonId = 1
        });

        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.FinishMainQuest(
            new FinishMainQuestRequest { QuestId = PlayableQuestId, IsRetired = false },
            FakeServerCallContext.For(UserId));

        EntityIUserQuest userQuest = Assert.Single(userDb.EntityIUserQuest);
        Assert.Equal((int)QuestStateType.CLEARED, userQuest.QuestStateType);
        Assert.Equal(2, userQuest.ClearCount);

        EntityIUserMainQuestMainFlowStatus mainFlowStatus = Assert.Single(userDb.EntityIUserMainQuestMainFlowStatus);
        Assert.Equal(PlayableLastSceneId, mainFlowStatus.CurrentQuestSceneId);
        Assert.Equal(PlayableLastSceneId, mainFlowStatus.HeadQuestSceneId);
        Assert.Equal(PlayableRouteId, mainFlowStatus.CurrentMainQuestRouteId);
        Assert.True(mainFlowStatus.IsReachedLastQuestScene);

        EntityIUserMainQuestSeasonRoute seasonRoute = Assert.Single(userDb.EntityIUserMainQuestSeasonRoute);
        Assert.Equal(PlayableRouteId, seasonRoute.MainQuestRouteId);
        Assert.Equal(PlayableSeasonId, seasonRoute.MainQuestSeasonId);

        EntityIUserMainQuestProgressStatus progressStatus = Assert.Single(userDb.EntityIUserMainQuestProgressStatus);
        Assert.Equal(0, progressStatus.CurrentQuestSceneId);
        Assert.Equal(0, progressStatus.HeadQuestSceneId);
        Assert.Equal(QuestFlowType.UNKNOWN, progressStatus.CurrentQuestFlowType);

        EntityIUserMainQuestFlowStatus flowStatus = Assert.Single(userDb.EntityIUserMainQuestFlowStatus);
        Assert.Equal(QuestFlowType.UNKNOWN, flowStatus.CurrentQuestFlowType);
    }

    [Fact]
    public async Task FinishMainQuest_ReplayFlow_DoesNotOverwriteSavedMainFlowPosition()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = PlayableQuestId,
            QuestStateType = (int)QuestStateType.ACTIVE,
            ClearCount = 1
        });
        userDb.EntityIUserMainQuestFlowStatus.Add(new EntityIUserMainQuestFlowStatus
        {
            UserId = UserId,
            CurrentQuestFlowType = QuestFlowType.REPLAY_FLOW
        });
        userDb.EntityIUserMainQuestMainFlowStatus.Add(new EntityIUserMainQuestMainFlowStatus
        {
            UserId = UserId,
            CurrentQuestSceneId = PlayableFirstSceneId,
            HeadQuestSceneId = PlayableFirstSceneId,
            CurrentMainQuestRouteId = PlayableRouteId
        });
        userDb.EntityIUserMainQuestReplayFlowStatus.Add(new EntityIUserMainQuestReplayFlowStatus
        {
            UserId = UserId,
            CurrentQuestSceneId = PlayableLastSceneId,
            CurrentHeadQuestSceneId = PlayableLastSceneId
        });
        userDb.EntityIUserMainQuestSeasonRoute.Add(new EntityIUserMainQuestSeasonRoute
        {
            UserId = UserId,
            MainQuestRouteId = PlayableRouteId,
            MainQuestSeasonId = PlayableSeasonId
        });

        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.FinishMainQuest(
            new FinishMainQuestRequest { QuestId = PlayableQuestId, IsRetired = false },
            FakeServerCallContext.For(UserId));

        EntityIUserMainQuestMainFlowStatus mainFlowStatus = Assert.Single(userDb.EntityIUserMainQuestMainFlowStatus);
        Assert.Equal(PlayableFirstSceneId, mainFlowStatus.CurrentQuestSceneId);
        Assert.Equal(PlayableFirstSceneId, mainFlowStatus.HeadQuestSceneId);
        Assert.Equal(PlayableRouteId, mainFlowStatus.CurrentMainQuestRouteId);

        EntityIUserMainQuestReplayFlowStatus replayFlowStatus = Assert.Single(userDb.EntityIUserMainQuestReplayFlowStatus);
        Assert.Equal(0, replayFlowStatus.CurrentQuestSceneId);
        Assert.Equal(0, replayFlowStatus.CurrentHeadQuestSceneId);
    }

    [Fact]
    public async Task StartMainQuest_BackgroundQuest_SyncsRouteSeasonAndSceneState()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();

        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.StartMainQuest(
            new StartMainQuestRequest { QuestId = BackgroundQuestId },
            FakeServerCallContext.For(UserId));

        EntityIUserQuest userQuest = Assert.Single(userDb.EntityIUserQuest);
        Assert.Equal((int)QuestStateType.CLEARED, userQuest.QuestStateType);

        EntityIUserMainQuestFlowStatus flowStatus = Assert.Single(userDb.EntityIUserMainQuestFlowStatus);
        Assert.Equal(QuestFlowType.MAIN_FLOW, flowStatus.CurrentQuestFlowType);

        EntityIUserMainQuestMainFlowStatus mainFlowStatus = Assert.Single(userDb.EntityIUserMainQuestMainFlowStatus);
        Assert.Equal(BackgroundSceneId, mainFlowStatus.CurrentQuestSceneId);
        Assert.Equal(BackgroundSceneId, mainFlowStatus.HeadQuestSceneId);
        Assert.Equal(BackgroundRouteId, mainFlowStatus.CurrentMainQuestRouteId);
        Assert.True(mainFlowStatus.IsReachedLastQuestScene);

        EntityIUserMainQuestSeasonRoute seasonRoute = Assert.Single(userDb.EntityIUserMainQuestSeasonRoute);
        Assert.Equal(BackgroundRouteId, seasonRoute.MainQuestRouteId);
        Assert.Equal(BackgroundSeasonId, seasonRoute.MainQuestSeasonId);
    }

    private static QuestServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(masterDb);
        store.Set(UserId, userDb);
        return new QuestServiceImpl(store, masterDb);
    }

    private static DarkMasterMemoryDatabase CreateMasterDb()
    {
        DarkMasterMemoryDatabase masterDb = new();

        masterDb.EntityMQuest.AddRange(
        [
            new EntityMQuest
            {
                QuestId = PlayableQuestId,
                IsRunInTheBackground = false,
                IsCountedAsQuest = true
            },
            new EntityMQuest
            {
                QuestId = BackgroundQuestId,
                IsRunInTheBackground = true,
                IsCountedAsQuest = false
            }
        ]);

        masterDb.EntityMQuestScene.AddRange(
        [
            new EntityMQuestScene { QuestSceneId = PlayableFirstSceneId, QuestId = PlayableQuestId, SortOrder = 1 },
            new EntityMQuestScene { QuestSceneId = PlayableLastSceneId, QuestId = PlayableQuestId, SortOrder = 2 },
            new EntityMQuestScene { QuestSceneId = BackgroundSceneId, QuestId = BackgroundQuestId, SortOrder = 1 }
        ]);

        masterDb.EntityMMainQuestRoute.AddRange(
        [
            new EntityMMainQuestRoute { MainQuestRouteId = PlayableRouteId, MainQuestSeasonId = PlayableSeasonId },
            new EntityMMainQuestRoute { MainQuestRouteId = BackgroundRouteId, MainQuestSeasonId = BackgroundSeasonId }
        ]);

        masterDb.EntityMMainQuestSequence.AddRange(
        [
            new EntityMMainQuestSequence { MainQuestSequenceId = 11, QuestId = PlayableQuestId, SortOrder = 1 },
            new EntityMMainQuestSequence { MainQuestSequenceId = 21, QuestId = BackgroundQuestId, SortOrder = 1 }
        ]);

        masterDb.EntityMMainQuestSequenceGroup.AddRange(
        [
            new EntityMMainQuestSequenceGroup { MainQuestSequenceGroupId = 111, MainQuestSequenceId = 11 },
            new EntityMMainQuestSequenceGroup { MainQuestSequenceGroupId = 211, MainQuestSequenceId = 21 }
        ]);

        masterDb.EntityMMainQuestChapter.AddRange(
        [
            new EntityMMainQuestChapter { MainQuestChapterId = 1111, MainQuestSequenceGroupId = 111, MainQuestRouteId = PlayableRouteId },
            new EntityMMainQuestChapter { MainQuestChapterId = 2111, MainQuestSequenceGroupId = 211, MainQuestRouteId = BackgroundRouteId }
        ]);

        return masterDb;
    }
}
