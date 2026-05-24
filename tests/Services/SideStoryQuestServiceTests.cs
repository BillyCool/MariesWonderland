using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.SideStoryQuest;
using MariesWonderland.Tests.Infrastructure;
using SideStoryQuestServiceImpl = MariesWonderland.Services.SideStoryQuestService;

namespace MariesWonderland.Tests.Services;

public class SideStoryQuestServiceTests
{
    private const long UserId = 1L;
    private const int QuestId = 100;
    private const int OtherQuestId = 200;
    private const int FirstSceneId = 1001;
    private const int SecondSceneId = 1002;
    private const int ThirdSceneId = 1003;

    [Fact]
    public async Task MoveSideStoryQuestProgress_ForNewQuest_UsesFirstSortedSceneAndCreatesTracking()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        SideStoryQuestServiceImpl service = CreateService(masterDb, userDb);

        await service.MoveSideStoryQuestProgress(
            new MoveSideStoryQuestRequest { SideStoryQuestId = QuestId },
            FakeServerCallContext.For(UserId));

        EntityIUserSideStoryQuestSceneProgressStatus activeProgress = Assert.Single(userDb.EntityIUserSideStoryQuestSceneProgressStatus);
        Assert.Equal(UserId, activeProgress.UserId);
        Assert.Equal(QuestId, activeProgress.CurrentSideStoryQuestId);
        Assert.Equal(FirstSceneId, activeProgress.CurrentSideStoryQuestSceneId);

        EntityIUserSideStoryQuest quest = Assert.Single(userDb.EntityIUserSideStoryQuest);
        Assert.Equal(UserId, quest.UserId);
        Assert.Equal(QuestId, quest.SideStoryQuestId);
        Assert.Equal(FirstSceneId, quest.HeadSideStoryQuestSceneId);
        Assert.Equal(1, quest.SideStoryQuestStateType);
    }

    [Fact]
    public async Task MoveSideStoryQuestProgress_ForExistingQuest_ResumesSavedHeadWithoutDuplicatingRecords()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        userDb.EntityIUserSideStoryQuest.Add(new EntityIUserSideStoryQuest
        {
            UserId = UserId,
            SideStoryQuestId = QuestId,
            HeadSideStoryQuestSceneId = ThirdSceneId,
            SideStoryQuestStateType = 7
        });
        userDb.EntityIUserSideStoryQuestSceneProgressStatus.Add(new EntityIUserSideStoryQuestSceneProgressStatus
        {
            UserId = UserId,
            CurrentSideStoryQuestId = OtherQuestId,
            CurrentSideStoryQuestSceneId = 9999
        });
        SideStoryQuestServiceImpl service = CreateService(masterDb, userDb);

        await service.MoveSideStoryQuestProgress(
            new MoveSideStoryQuestRequest { SideStoryQuestId = QuestId },
            FakeServerCallContext.For(UserId));

        EntityIUserSideStoryQuestSceneProgressStatus activeProgress = Assert.Single(userDb.EntityIUserSideStoryQuestSceneProgressStatus);
        Assert.Equal(QuestId, activeProgress.CurrentSideStoryQuestId);
        Assert.Equal(ThirdSceneId, activeProgress.CurrentSideStoryQuestSceneId);

        EntityIUserSideStoryQuest quest = Assert.Single(userDb.EntityIUserSideStoryQuest);
        Assert.Equal(ThirdSceneId, quest.HeadSideStoryQuestSceneId);
        Assert.Equal(7, quest.SideStoryQuestStateType);
    }

    [Fact]
    public async Task MoveSideStoryQuestProgress_WhenQuestHasNoScenes_LeavesCurrentSceneAtZero()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        SideStoryQuestServiceImpl service = CreateService(masterDb, userDb);

        await service.MoveSideStoryQuestProgress(
            new MoveSideStoryQuestRequest { SideStoryQuestId = 999 },
            FakeServerCallContext.For(UserId));

        EntityIUserSideStoryQuestSceneProgressStatus activeProgress = Assert.Single(userDb.EntityIUserSideStoryQuestSceneProgressStatus);
        Assert.Equal(999, activeProgress.CurrentSideStoryQuestId);
        Assert.Equal(0, activeProgress.CurrentSideStoryQuestSceneId);

        EntityIUserSideStoryQuest quest = Assert.Single(userDb.EntityIUserSideStoryQuest);
        Assert.Equal(999, quest.SideStoryQuestId);
        Assert.Equal(0, quest.HeadSideStoryQuestSceneId);
    }

    [Fact]
    public async Task UpdateSideStoryQuestSceneProgress_WhenSceneAdvances_UpdatesCurrentSceneAndHead()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        userDb.EntityIUserSideStoryQuest.Add(new EntityIUserSideStoryQuest
        {
            UserId = UserId,
            SideStoryQuestId = QuestId,
            HeadSideStoryQuestSceneId = FirstSceneId,
            SideStoryQuestStateType = 1
        });
        SideStoryQuestServiceImpl service = CreateService(masterDb, userDb);

        await service.UpdateSideStoryQuestSceneProgress(
            new UpdateSideStoryQuestSceneProgressRequest
            {
                SideStoryQuestId = QuestId,
                SideStoryQuestSceneId = ThirdSceneId
            },
            FakeServerCallContext.For(UserId));

        EntityIUserSideStoryQuestSceneProgressStatus activeProgress = Assert.Single(userDb.EntityIUserSideStoryQuestSceneProgressStatus);
        Assert.Equal(UserId, activeProgress.UserId);
        Assert.Equal(0, activeProgress.CurrentSideStoryQuestId);
        Assert.Equal(ThirdSceneId, activeProgress.CurrentSideStoryQuestSceneId);

        EntityIUserSideStoryQuest quest = Assert.Single(userDb.EntityIUserSideStoryQuest);
        Assert.Equal(ThirdSceneId, quest.HeadSideStoryQuestSceneId);
    }

    [Fact]
    public async Task UpdateSideStoryQuestSceneProgress_WhenSceneDoesNotAdvance_PreservesSavedHead()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        userDb.EntityIUserSideStoryQuest.Add(new EntityIUserSideStoryQuest
        {
            UserId = UserId,
            SideStoryQuestId = QuestId,
            HeadSideStoryQuestSceneId = ThirdSceneId,
            SideStoryQuestStateType = 1
        });
        userDb.EntityIUserSideStoryQuestSceneProgressStatus.Add(new EntityIUserSideStoryQuestSceneProgressStatus
        {
            UserId = UserId,
            CurrentSideStoryQuestId = QuestId,
            CurrentSideStoryQuestSceneId = ThirdSceneId
        });
        SideStoryQuestServiceImpl service = CreateService(masterDb, userDb);

        await service.UpdateSideStoryQuestSceneProgress(
            new UpdateSideStoryQuestSceneProgressRequest
            {
                SideStoryQuestId = QuestId,
                SideStoryQuestSceneId = SecondSceneId
            },
            FakeServerCallContext.For(UserId));

        EntityIUserSideStoryQuestSceneProgressStatus activeProgress = Assert.Single(userDb.EntityIUserSideStoryQuestSceneProgressStatus);
        Assert.Equal(QuestId, activeProgress.CurrentSideStoryQuestId);
        Assert.Equal(SecondSceneId, activeProgress.CurrentSideStoryQuestSceneId);

        EntityIUserSideStoryQuest quest = Assert.Single(userDb.EntityIUserSideStoryQuest);
        Assert.Equal(ThirdSceneId, quest.HeadSideStoryQuestSceneId);
    }

    [Fact]
    public async Task UpdateSideStoryQuestSceneProgress_WithoutQuestTracking_OnlyUpdatesActiveScene()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        SideStoryQuestServiceImpl service = CreateService(masterDb, userDb);

        await service.UpdateSideStoryQuestSceneProgress(
            new UpdateSideStoryQuestSceneProgressRequest
            {
                SideStoryQuestId = QuestId,
                SideStoryQuestSceneId = SecondSceneId
            },
            FakeServerCallContext.For(UserId));

        EntityIUserSideStoryQuestSceneProgressStatus activeProgress = Assert.Single(userDb.EntityIUserSideStoryQuestSceneProgressStatus);
        Assert.Equal(UserId, activeProgress.UserId);
        Assert.Equal(0, activeProgress.CurrentSideStoryQuestId);
        Assert.Equal(SecondSceneId, activeProgress.CurrentSideStoryQuestSceneId);
        Assert.Empty(userDb.EntityIUserSideStoryQuest);
    }

    private static SideStoryQuestServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(masterDb);
        store.Set(UserId, userDb);
        return new SideStoryQuestServiceImpl(store, masterDb);
    }

    private static DarkMasterMemoryDatabase CreateMasterDb()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMSideStoryQuestScene.AddRange(
        [
            new EntityMSideStoryQuestScene
            {
                SideStoryQuestId = QuestId,
                SideStoryQuestSceneId = SecondSceneId,
                SortOrder = 2
            },
            new EntityMSideStoryQuestScene
            {
                SideStoryQuestId = QuestId,
                SideStoryQuestSceneId = FirstSceneId,
                SortOrder = 1
            },
            new EntityMSideStoryQuestScene
            {
                SideStoryQuestId = QuestId,
                SideStoryQuestSceneId = ThirdSceneId,
                SortOrder = 3
            },
            new EntityMSideStoryQuestScene
            {
                SideStoryQuestId = OtherQuestId,
                SideStoryQuestSceneId = 2001,
                SortOrder = 1
            }
        ]);
        return masterDb;
    }
}
