using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Quest;
using MariesWonderland.Quests;
using MariesWonderland.Tests.Infrastructure;
using QuestService = MariesWonderland.Services.QuestService;

namespace MariesWonderland.Tests.Quests;

public class MenuPickQuestTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private (QuestService Service, DarkUserMemoryDatabase UserDb, long UserId) CreateService()
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("menu-pick-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        QuestCampaignHelper campaigns = new(MasterDb);
        QuestService service = new(store, MasterDb, new QuestEngine(MasterDb, GameConfig, campaigns), campaigns, TimeProvider.System);
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
}
