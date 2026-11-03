using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
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
    private const int ExtraQuestId = 301;
    private const int ExtraFirstSceneId = 3001;
    private const int ExtraSecondSceneId = 3002;
    private const int EventQuestId = 401;
    private const int EventBattleSceneId = 4001;
    private const int EventSecondSceneId = 4002;
    private const int EventQuestChapterId = 44;
    private const int ChoiceGroupingId = 501;
    private const int MainFlowChoiceEffectId = 601;
    private const int AnotherRouteChoiceEffectId = 602;
    private const int SideEffectChoiceEffectId = 603;
    private const int SideEffectCostumeEffectGroupId = 611;
    private const int SideEffectWeaponEffectGroupId = 612;
    private const int SideEffectCostumeId = 701;
    private const int SideEffectCharacterId = 702;
    private const int SideEffectWeaponId = 703;
    private const int SideEffectWeaponAbilityGroupId = 704;
    private const int SideEffectWeaponSkillGroupId = 705;
    private const int TowerChapterId = 301;
    private const int AutoSalePartsId = 9301;
    private const int AutoSaleGoldItemId = 1;

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

        // One route row per season, holding the first route taken: the quest's season gets its own row and the
        // existing season 1 row stays as it was.
        EntityIUserMainQuestSeasonRoute seasonRoute = Assert.Single(userDb.EntityIUserMainQuestSeasonRoute, r => r.MainQuestSeasonId == PlayableSeasonId);
        Assert.Equal(PlayableRouteId, seasonRoute.MainQuestRouteId);
        Assert.Contains(userDb.EntityIUserMainQuestSeasonRoute, r => r.MainQuestSeasonId == 1 && r.MainQuestRouteId == 1);

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
        // The replay position stays (the map places its chapter points from it) and the flow stays a replay.
        Assert.Equal(PlayableLastSceneId, replayFlowStatus.CurrentQuestSceneId);
        Assert.Equal(PlayableLastSceneId, replayFlowStatus.CurrentHeadQuestSceneId);
        Assert.Equal(QuestFlowType.REPLAY_FLOW, Assert.Single(userDb.EntityIUserMainQuestFlowStatus).CurrentQuestFlowType);
    }

    [Fact]
    public async Task StartMainQuest_BackgroundQuest_SyncsRouteSeasonAndSceneState()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();

        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.StartMainQuest(
            new StartMainQuestRequest { QuestId = BackgroundQuestId, IsMainFlow = true },
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

    [Fact]
    public async Task SetQuestSceneChoice_AnotherRouteReplay_StoresChoiceHistoryAndRouteState()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        userDb.EntityIUserQuestSceneChoice.Add(new EntityIUserQuestSceneChoice
        {
            UserId = UserId,
            QuestSceneChoiceGroupingId = ChoiceGroupingId,
            QuestSceneChoiceEffectId = MainFlowChoiceEffectId
        });
        userDb.EntityIUserMainQuestFlowStatus.Add(new EntityIUserMainQuestFlowStatus { UserId = UserId });
        userDb.EntityIUserMainQuestMainFlowStatus.Add(new EntityIUserMainQuestMainFlowStatus { UserId = UserId });
        userDb.EntitySQuestSession.Add(new EntitySQuestSession { UserId = UserId, QuestId = PlayableQuestId });

        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.SetQuestSceneChoice(
            new SetQuestSceneChoiceRequest
            {
                QuestSceneId = PlayableFirstSceneId,
                ChoiceNumber = 2,
                QuestFlowType = (int)QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW
            },
            FakeServerCallContext.For(UserId));

        EntityIUserQuestSceneChoice userChoice = Assert.Single(userDb.EntityIUserQuestSceneChoice);
        Assert.Equal(ChoiceGroupingId, userChoice.QuestSceneChoiceGroupingId);
        Assert.Equal(AnotherRouteChoiceEffectId, userChoice.QuestSceneChoiceEffectId);

        EntityIUserQuestSceneChoiceHistory choiceHistory = Assert.Single(userDb.EntityIUserQuestSceneChoiceHistory);
        Assert.Equal(AnotherRouteChoiceEffectId, choiceHistory.QuestSceneChoiceEffectId);
        Assert.True(choiceHistory.ChoiceDatetime > 0);

        EntityIUserMainQuestFlowStatus flowStatus = Assert.Single(userDb.EntityIUserMainQuestFlowStatus);
        Assert.Equal(QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW, flowStatus.CurrentQuestFlowType);

        EntityIUserMainQuestMainFlowStatus mainFlowStatus = Assert.Single(userDb.EntityIUserMainQuestMainFlowStatus);
        Assert.Equal(PlayableRouteId, mainFlowStatus.CurrentMainQuestRouteId);

        EntityIUserMainQuestSeasonRoute seasonRoute = Assert.Single(userDb.EntityIUserMainQuestSeasonRoute);
        Assert.Equal(PlayableRouteId, seasonRoute.MainQuestRouteId);
        Assert.Equal(PlayableSeasonId, seasonRoute.MainQuestSeasonId);

        EntitySQuestSession questSession = Assert.Single(userDb.EntitySQuestSession);
        Assert.Equal(QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW, questSession.CurrentQuestFlowType);
    }

    [Fact]
    public async Task SetQuestSceneChoice_FirstSelection_GrantsConfiguredCostumeAndWeaponSideEffects()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();

        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.SetQuestSceneChoice(
            new SetQuestSceneChoiceRequest
            {
                QuestSceneId = PlayableFirstSceneId,
                ChoiceNumber = 3,
                QuestFlowType = (int)QuestFlowType.MAIN_FLOW
            },
            FakeServerCallContext.For(UserId));

        EntityIUserCostume costume = Assert.Single(userDb.EntityIUserCostume);
        Assert.Equal(SideEffectCostumeId, costume.CostumeId);

        EntityIUserCharacter character = Assert.Single(userDb.EntityIUserCharacter);
        Assert.Equal(SideEffectCharacterId, character.CharacterId);

        EntityIUserCostumeActiveSkill costumeSkill = Assert.Single(userDb.EntityIUserCostumeActiveSkill);
        Assert.Equal(costume.UserCostumeUuid, costumeSkill.UserCostumeUuid);

        EntityIUserWeapon weapon = Assert.Single(userDb.EntityIUserWeapon);
        Assert.Equal(SideEffectWeaponId, weapon.WeaponId);

        EntityIUserWeaponNote weaponNote = Assert.Single(userDb.EntityIUserWeaponNote);
        Assert.Equal(SideEffectWeaponId, weaponNote.WeaponId);

        EntityIUserWeaponAbility weaponAbility = Assert.Single(userDb.EntityIUserWeaponAbility);
        Assert.Equal(weapon.UserWeaponUuid, weaponAbility.UserWeaponUuid);

        EntityIUserWeaponSkill weaponSkill = Assert.Single(userDb.EntityIUserWeaponSkill);
        Assert.Equal(weapon.UserWeaponUuid, weaponSkill.UserWeaponUuid);
    }

    [Fact]
    public async Task SetQuestSceneChoice_RepeatSelection_DoesNotDuplicateSideEffects()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();

        QuestServiceImpl service = CreateService(masterDb, userDb);

        SetQuestSceneChoiceRequest request = new()
        {
            QuestSceneId = PlayableFirstSceneId,
            ChoiceNumber = 3,
            QuestFlowType = (int)QuestFlowType.MAIN_FLOW
        };

        await service.SetQuestSceneChoice(request, FakeServerCallContext.For(UserId));
        long firstChoiceTimestamp = Assert.Single(userDb.EntityIUserQuestSceneChoiceHistory).ChoiceDatetime;

        await Task.Delay(1);
        await service.SetQuestSceneChoice(request, FakeServerCallContext.For(UserId));

        Assert.Single(userDb.EntityIUserCostume);
        Assert.Single(userDb.EntityIUserCharacter);
        Assert.Single(userDb.EntityIUserCostumeActiveSkill);
        Assert.Single(userDb.EntityIUserWeapon);
        Assert.Single(userDb.EntityIUserWeaponNote);
        Assert.Single(userDb.EntityIUserWeaponAbility);
        Assert.Single(userDb.EntityIUserWeaponSkill);

        EntityIUserQuestSceneChoiceHistory choiceHistory = Assert.Single(userDb.EntityIUserQuestSceneChoiceHistory);
        Assert.Equal(SideEffectChoiceEffectId, choiceHistory.QuestSceneChoiceEffectId);
        Assert.True(choiceHistory.ChoiceDatetime >= firstChoiceTimestamp);
    }

    [Fact]
    public async Task UpdateReplayFlowSceneProgress_WithNoStart_PutsTheFlowInReplayAndTheCageOff()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        userDb.EntityIUserMainQuestFlowStatus.Add(new EntityIUserMainQuestFlowStatus
        {
            UserId = UserId,
            CurrentQuestFlowType = QuestFlowType.MAIN_FLOW
        });
        userDb.EntityIUserPortalCageStatus.Add(new EntityIUserPortalCageStatus { UserId = UserId, IsCurrentProgress = true });

        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.UpdateReplayFlowSceneProgress(
            new UpdateReplayFlowSceneProgressRequest { QuestSceneId = PlayableLastSceneId },
            FakeServerCallContext.For(UserId));

        Assert.Equal(QuestFlowType.REPLAY_FLOW, Assert.Single(userDb.EntityIUserMainQuestFlowStatus).CurrentQuestFlowType);
        Assert.False(Assert.Single(userDb.EntityIUserPortalCageStatus).IsCurrentProgress);
        Assert.Equal(PlayableLastSceneId, Assert.Single(userDb.EntityIUserMainQuestReplayFlowStatus).CurrentQuestSceneId);
    }

    [Fact]
    public async Task ReplayStart_OfAStoryOnlyQuest_MarksItClearedAtOnce()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.StartMainQuest(
            new StartMainQuestRequest { QuestId = BackgroundQuestId, UserDeckNumber = 4, IsReplayFlow = true },
            FakeServerCallContext.For(UserId));
        await service.StartMainQuest(
            new StartMainQuestRequest { QuestId = BackgroundQuestId, UserDeckNumber = 4, IsReplayFlow = true },
            FakeServerCallContext.For(UserId));

        EntityIUserQuest quest = Assert.Single(userDb.EntityIUserQuest, q => q.QuestId == BackgroundQuestId);
        Assert.Equal((int)QuestStateType.CLEARED, quest.QuestStateType);
        Assert.Equal(1, quest.ClearCount);
    }

    [Fact]
    public async Task ReplayStart_ThenMainQuestSceneProgress_KeepsTheReplayFlow()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = PlayableQuestId,
            QuestStateType = (int)QuestStateType.CLEARED
        });

        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.StartMainQuest(
            new StartMainQuestRequest { QuestId = PlayableQuestId, UserDeckNumber = 4, IsReplayFlow = true },
            FakeServerCallContext.For(UserId));
        await service.UpdateMainQuestSceneProgress(
            new UpdateMainQuestSceneProgressRequest { QuestSceneId = PlayableLastSceneId },
            FakeServerCallContext.For(UserId));

        Assert.Equal(QuestFlowType.REPLAY_FLOW, Assert.Single(userDb.EntityIUserMainQuestFlowStatus).CurrentQuestFlowType);
        Assert.Equal(QuestFlowType.REPLAY_FLOW, Assert.Single(userDb.EntityIUserMainQuestProgressStatus).CurrentQuestFlowType);
        Assert.Equal(PlayableLastSceneId, Assert.Single(userDb.EntityIUserMainQuestReplayFlowStatus).CurrentQuestSceneId);
    }

    [Fact]
    public async Task StartReplayMainQuest_UpdateReplayProgress_RestartRestoresReplayScene()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = PlayableQuestId,
            QuestStateType = (int)QuestStateType.CLEARED
        });

        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.StartMainQuest(
            new StartMainQuestRequest { QuestId = PlayableQuestId, UserDeckNumber = 4, IsReplayFlow = true },
            FakeServerCallContext.For(UserId));
        await service.UpdateReplayFlowSceneProgress(
            new UpdateReplayFlowSceneProgressRequest { QuestSceneId = PlayableLastSceneId },
            FakeServerCallContext.For(UserId));

        RestartMainQuestResponse response = await service.RestartMainQuest(
            new RestartMainQuestRequest { QuestId = PlayableQuestId },
            FakeServerCallContext.For(UserId));

        Assert.Equal(4, response.DeckNumber);

        EntityIUserMainQuestFlowStatus flowStatus = Assert.Single(userDb.EntityIUserMainQuestFlowStatus);
        Assert.Equal(QuestFlowType.REPLAY_FLOW, flowStatus.CurrentQuestFlowType);

        EntityIUserMainQuestReplayFlowStatus replayFlowStatus = Assert.Single(userDb.EntityIUserMainQuestReplayFlowStatus);
        Assert.Equal(PlayableLastSceneId, replayFlowStatus.CurrentQuestSceneId);
        Assert.Equal(PlayableLastSceneId, replayFlowStatus.CurrentHeadQuestSceneId);

        EntityIUserMainQuestSeasonRoute seasonRoute = Assert.Single(userDb.EntityIUserMainQuestSeasonRoute);
        Assert.Equal(PlayableRouteId, seasonRoute.MainQuestRouteId);
        Assert.Equal(PlayableSeasonId, seasonRoute.MainQuestSeasonId);

        EntitySQuestSession questSession = Assert.Single(userDb.EntitySQuestSession);
        Assert.Equal(PlayableLastSceneId, questSession.CurrentQuestSceneId);
        Assert.Equal(QuestFlowType.REPLAY_FLOW, questSession.CurrentQuestFlowType);
    }

    [Fact]
    public async Task StartExtraQuest_UpdateProgress_RestartRestoresQuestSessionScene()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.StartExtraQuest(
            new StartExtraQuestRequest { QuestId = ExtraQuestId, UserDeckNumber = 7 },
            FakeServerCallContext.For(UserId));
        await service.UpdateExtraQuestSceneProgress(
            new UpdateExtraQuestSceneProgressRequest { QuestSceneId = ExtraSecondSceneId },
            FakeServerCallContext.For(UserId));

        RestartExtraQuestResponse response = await service.RestartExtraQuest(
            new RestartExtraQuestRequest { QuestId = ExtraQuestId },
            FakeServerCallContext.For(UserId));

        Assert.Equal(7, response.DeckNumber);

        EntityIUserExtraQuestProgressStatus progressStatus = Assert.Single(userDb.EntityIUserExtraQuestProgressStatus);
        Assert.Equal(ExtraQuestId, progressStatus.CurrentQuestId);
        Assert.Equal(ExtraSecondSceneId, progressStatus.CurrentQuestSceneId);
        Assert.Equal(ExtraSecondSceneId, progressStatus.HeadQuestSceneId);

        EntitySQuestSession questSession = Assert.Single(userDb.EntitySQuestSession, s => s.QuestId == ExtraQuestId);
        Assert.Equal(ExtraSecondSceneId, questSession.CurrentQuestSceneId);
        Assert.Equal(ExtraSecondSceneId, questSession.HeadQuestSceneId);
    }

    [Fact]
    public async Task StartEventQuest_UpdateProgress_RestartRestoresBattleOnlySessionState()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.StartEventQuest(
            new StartEventQuestRequest
            {
                EventQuestChapterId = EventQuestChapterId,
                QuestId = EventQuestId,
                UserDeckNumber = 8,
                IsBattleOnly = true
            },
            FakeServerCallContext.For(UserId));
        await service.UpdateEventQuestSceneProgress(
            new UpdateEventQuestSceneProgressRequest { QuestSceneId = EventSecondSceneId },
            FakeServerCallContext.For(UserId));

        RestartEventQuestResponse response = await service.RestartEventQuest(
            new RestartEventQuestRequest
            {
                EventQuestChapterId = EventQuestChapterId + 1,
                QuestId = EventQuestId
            },
            FakeServerCallContext.For(UserId));

        Assert.Equal(8, response.DeckNumber);

        EntityIUserEventQuestProgressStatus progressStatus = Assert.Single(userDb.EntityIUserEventQuestProgressStatus);
        Assert.Equal(EventQuestChapterId, progressStatus.CurrentEventQuestChapterId);
        Assert.Equal(EventQuestId, progressStatus.CurrentQuestId);
        Assert.Equal(EventSecondSceneId, progressStatus.CurrentQuestSceneId);
        Assert.Equal(EventSecondSceneId, progressStatus.HeadQuestSceneId);

        EntityIUserQuest userQuest = Assert.Single(userDb.EntityIUserQuest, q => q.QuestId == EventQuestId);
        Assert.True(userQuest.IsBattleOnly);

        EntitySQuestSession questSession = Assert.Single(userDb.EntitySQuestSession, s => s.QuestId == EventQuestId);
        Assert.True(questSession.IsBattleOnly);
        Assert.Equal(EventQuestChapterId, questSession.CurrentEventQuestChapterId);
        Assert.Equal(EventSecondSceneId, questSession.CurrentQuestSceneId);
    }

    [Fact]
    public async Task FinishMainQuest_AppliesCampaignDropRateAndBonusDrops()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();

        EntityMQuest quest = Assert.Single(masterDb.EntityMQuest, q => q.QuestId == PlayableQuestId);
        quest.QuestPickupRewardGroupId = 9100;

        masterDb.EntityMQuestPickupRewardGroup.Add(new EntityMQuestPickupRewardGroup
        {
            QuestPickupRewardGroupId = 9100,
            BattleDropRewardId = 9101
        });
        masterDb.EntityMBattleDropReward.Add(new EntityMBattleDropReward
        {
            BattleDropRewardId = 9101,
            PossessionType = PossessionType.MATERIAL,
            PossessionId = 9102,
            Count = 2
        });

        masterDb.EntityMQuestCampaignTargetGroup.Add(new EntityMQuestCampaignTargetGroup
        {
            QuestCampaignTargetGroupId = 9103,
            QuestCampaignTargetIndex = 1,
            QuestCampaignTargetType = QuestCampaignTargetType.MAIN_QUEST_CHAPTER_ID,
            QuestCampaignTargetValue = 1111
        });
        masterDb.EntityMQuestCampaignEffectGroup.Add(new EntityMQuestCampaignEffectGroup
        {
            QuestCampaignEffectGroupId = 9104,
            QuestCampaignEffectType = QuestCampaignEffectType.DROP_RATE,
            QuestCampaignEffectValue = 500
        });
        masterDb.EntityMQuestCampaign.Add(new EntityMQuestCampaign
        {
            QuestCampaignId = 9105,
            QuestCampaignTargetGroupId = 9103,
            QuestCampaignEffectGroupId = 9104,
            StartDatetime = 0,
            EndDatetime = long.MaxValue,
            TargetUserStatusType = TargetUserStatusType.ALL
        });

        masterDb.EntityMQuestCampaignEffectGroup.Add(new EntityMQuestCampaignEffectGroup
        {
            QuestCampaignEffectGroupId = 9106,
            QuestCampaignEffectType = QuestCampaignEffectType.DROP_ITEM_ADD,
            QuestCampaignTargetItemGroupId = 9107
        });
        masterDb.EntityMQuestCampaignTargetItemGroup.Add(new EntityMQuestCampaignTargetItemGroup
        {
            QuestCampaignTargetItemGroupId = 9107,
            TargetIndex = 1,
            PossessionType = PossessionType.MATERIAL,
            PossessionId = 9108,
            Count = 4
        });
        masterDb.EntityMQuestCampaign.Add(new EntityMQuestCampaign
        {
            QuestCampaignId = 9109,
            QuestCampaignTargetGroupId = 9103,
            QuestCampaignEffectGroupId = 9106,
            StartDatetime = 0,
            EndDatetime = long.MaxValue,
            TargetUserStatusType = TargetUserStatusType.ALL
        });

        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = PlayableQuestId,
            QuestStateType = (int)QuestStateType.ACTIVE,
            ClearCount = 1
        });

        QuestServiceImpl service = CreateService(masterDb, userDb);

        FinishMainQuestResponse response = await service.FinishMainQuest(
            new FinishMainQuestRequest { QuestId = PlayableQuestId },
            FakeServerCallContext.For(UserId));

        Assert.Equal(
            ["5:9102:3", "5:9108:4"],
            response.DropReward.Select(reward => $"{reward.PossessionType}:{reward.PossessionId}:{reward.Count}").ToArray());
        Assert.Equal(
            ["9102:3", "9108:4"],
            userDb.EntityIUserMaterial.OrderBy(material => material.MaterialId).Select(material => $"{material.MaterialId}:{material.Count}").ToArray());
    }

    [Fact]
    public async Task FinishEventQuest_UsesStoredChapterToResolveCampaignEventType()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();

        EntityMQuest quest = Assert.Single(masterDb.EntityMQuest, q => q.QuestId == EventQuestId);
        quest.QuestPickupRewardGroupId = 9200;

        masterDb.EntityMQuestPickupRewardGroup.Add(new EntityMQuestPickupRewardGroup
        {
            QuestPickupRewardGroupId = 9200,
            BattleDropRewardId = 9201
        });
        masterDb.EntityMBattleDropReward.Add(new EntityMBattleDropReward
        {
            BattleDropRewardId = 9201,
            PossessionType = PossessionType.MATERIAL,
            PossessionId = 9202,
            Count = 2
        });
        masterDb.EntityMEventQuestChapter.Add(new EntityMEventQuestChapter
        {
            EventQuestChapterId = EventQuestChapterId,
            EventQuestType = EventQuestType.MARATHON
        });
        masterDb.EntityMQuestCampaignTargetGroup.Add(new EntityMQuestCampaignTargetGroup
        {
            QuestCampaignTargetGroupId = 9203,
            QuestCampaignTargetIndex = 1,
            QuestCampaignTargetType = QuestCampaignTargetType.EVENT_QUEST_TYPE,
            QuestCampaignTargetValue = (int)EventQuestType.MARATHON
        });
        masterDb.EntityMQuestCampaignEffectGroup.Add(new EntityMQuestCampaignEffectGroup
        {
            QuestCampaignEffectGroupId = 9204,
            QuestCampaignEffectType = QuestCampaignEffectType.DROP_RATE,
            QuestCampaignEffectValue = 500
        });
        masterDb.EntityMQuestCampaign.Add(new EntityMQuestCampaign
        {
            QuestCampaignId = 9205,
            QuestCampaignTargetGroupId = 9203,
            QuestCampaignEffectGroupId = 9204,
            StartDatetime = 0,
            EndDatetime = long.MaxValue,
            TargetUserStatusType = TargetUserStatusType.ALL
        });

        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = EventQuestId,
            QuestStateType = (int)QuestStateType.ACTIVE,
            ClearCount = 1
        });
        userDb.EntitySQuestSession.Add(new EntitySQuestSession
        {
            UserId = UserId,
            QuestId = EventQuestId,
            CurrentEventQuestChapterId = EventQuestChapterId
        });

        QuestServiceImpl service = CreateService(masterDb, userDb);

        FinishEventQuestResponse response = await service.FinishEventQuest(
            new FinishEventQuestRequest { QuestId = EventQuestId },
            FakeServerCallContext.For(UserId));

        QuestReward reward = Assert.Single(response.DropReward);
        Assert.Equal(9202, reward.PossessionId);
        Assert.Equal(3, reward.Count);
    }

    [Fact]
    public async Task FinishMainQuest_AutoSellsConfiguredMemoirDrops()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();

        EntityMQuest quest = Assert.Single(masterDb.EntityMQuest, q => q.QuestId == PlayableQuestId);
        quest.QuestPickupRewardGroupId = 9300;

        masterDb.EntityMQuestPickupRewardGroup.Add(new EntityMQuestPickupRewardGroup
        {
            QuestPickupRewardGroupId = 9300,
            BattleDropRewardId = 9302
        });
        masterDb.EntityMBattleDropReward.Add(new EntityMBattleDropReward
        {
            BattleDropRewardId = 9302,
            PossessionType = PossessionType.PARTS,
            PossessionId = AutoSalePartsId,
            Count = 1
        });
        masterDb.EntityMParts.Add(new EntityMParts
        {
            PartsId = AutoSalePartsId,
            RarityType = RarityType.RARE,
            PartsGroupId = 93,
            PartsStatusMainLotteryGroupId = 21,
            PartsInitialLotteryId = 2
        });
        masterDb.EntityMPartsRarity.Add(new EntityMPartsRarity
        {
            RarityType = RarityType.RARE,
            SellPriceNumericalFunctionId = 9303
        });
        masterDb.EntityMNumericalFunction.Add(new EntityMNumericalFunction
        {
            NumericalFunctionId = 9303,
            NumericalFunctionType = NumericalFunctionType.LINEAR,
            NumericalFunctionParameterGroupId = 9304
        });
        masterDb.EntityMNumericalFunctionParameterGroup.AddRange(
        [
            new EntityMNumericalFunctionParameterGroup
            {
                NumericalFunctionParameterGroupId = 9304,
                ParameterIndex = 0,
                ParameterValue = 0
            },
            new EntityMNumericalFunctionParameterGroup
            {
                NumericalFunctionParameterGroupId = 9304,
                ParameterIndex = 1,
                ParameterValue = 25
            }
        ]);

        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = PlayableQuestId,
            QuestStateType = (int)QuestStateType.ACTIVE,
            ClearCount = 1
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = AutoSaleGoldItemId,
            Count = 10
        });
        userDb.EntityIUserAutoSaleSettingDetail.Add(new EntityIUserAutoSaleSettingDetail
        {
            UserId = UserId,
            PossessionAutoSaleItemType = 1,
            PossessionAutoSaleItemValue = "20,2"
        });
        masterDb.EntityMConfig.Add(new EntityMConfig { ConfigKey = "CONSUMABLE_ITEM_ID_FOR_GOLD", Value = AutoSaleGoldItemId.ToString() });

        QuestServiceImpl service = CreateService(masterDb, userDb);

        FinishMainQuestResponse response = await service.FinishMainQuest(
            new FinishMainQuestRequest { QuestId = PlayableQuestId },
            FakeServerCallContext.For(UserId));

        QuestReward reward = Assert.Single(response.DropReward);
        Assert.Equal((int)PossessionType.PARTS, reward.PossessionType);
        Assert.Equal(AutoSalePartsId, reward.PossessionId);
        Assert.Equal(1, reward.Count);
        Assert.True(reward.IsAutoSale);
        Assert.Null(reward.Parts);
        Assert.Empty(userDb.EntityIUserParts);
        Assert.Equal(35, Assert.Single(userDb.EntityIUserConsumableItem).Count);
    }

    [Fact]
    public async Task FinishMainQuest_KeptMemoirDrop_CarriesItsStatsAndRarityReveal()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();

        EntityMQuest quest = Assert.Single(masterDb.EntityMQuest, q => q.QuestId == PlayableQuestId);
        quest.QuestPickupRewardGroupId = 9400;
        masterDb.EntityMQuestPickupRewardGroup.Add(new EntityMQuestPickupRewardGroup { QuestPickupRewardGroupId = 9400, BattleDropRewardId = 9402 });
        masterDb.EntityMBattleDropReward.Add(new EntityMBattleDropReward
        {
            BattleDropRewardId = 9402,
            PossessionType = PossessionType.PARTS,
            PossessionId = AutoSalePartsId,
            Count = 1
        });
        masterDb.EntityMParts.Add(new EntityMParts
        {
            PartsId = AutoSalePartsId,
            RarityType = RarityType.S_RARE,
            PartsGroupId = 94,
            PartsStatusMainLotteryGroupId = 21
        });
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = PlayableQuestId,
            QuestStateType = (int)QuestStateType.ACTIVE,
            ClearCount = 1
        });

        QuestServiceImpl service = CreateService(masterDb, userDb);

        FinishMainQuestResponse response = await service.FinishMainQuest(
            new FinishMainQuestRequest { QuestId = PlayableQuestId },
            FakeServerCallContext.For(UserId));

        QuestReward reward = Assert.Single(response.DropReward);
        EntityIUserParts granted = Assert.Single(userDb.EntityIUserParts);
        Assert.False(reward.IsAutoSale);
        Assert.Equal(RewardEffectHelper.SRare, reward.RewardEffectId);
        Assert.NotNull(reward.Parts);
        Assert.Equal(granted.Level, reward.Parts.Level);
        Assert.Equal(granted.PartsStatusMainId, reward.Parts.PartsStatusMainId);
        Assert.Empty(reward.Parts.PartsStatusSub);
    }

    [Fact]
    public async Task StartAndFinishMainQuest_AutoOrbitTracksCountersUntilFinishAutoOrbitClearsState()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.StartMainQuest(
            new StartMainQuestRequest { QuestId = PlayableQuestId, MaxAutoOrbitCount = 2 },
            FakeServerCallContext.For(UserId));

        EntityIUserQuestAutoOrbit autoOrbit = Assert.Single(userDb.EntityIUserQuestAutoOrbit);
        Assert.Equal(QuestType.MAIN_QUEST, autoOrbit.QuestType);
        Assert.Equal(PlayableQuestId, autoOrbit.QuestId);
        Assert.Equal(2, autoOrbit.MaxAutoOrbitCount);
        Assert.Equal(0, autoOrbit.ClearedAutoOrbitCount);

        FinishMainQuestResponse finishResponse = await service.FinishMainQuest(
            new FinishMainQuestRequest { QuestId = PlayableQuestId, IsAutoOrbit = true },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(finishResponse.AutoOrbitReward);
        Assert.Empty(finishResponse.AutoOrbitReward.DropReward);

        autoOrbit = Assert.Single(userDb.EntityIUserQuestAutoOrbit);
        Assert.Equal(1, autoOrbit.ClearedAutoOrbitCount);
        Assert.True(autoOrbit.LastClearDatetime > 0);

        FinishAutoOrbitResponse autoOrbitResponse = await service.FinishAutoOrbit(new Google.Protobuf.WellKnownTypes.Empty(), FakeServerCallContext.For(UserId));

        Assert.NotNull(autoOrbitResponse.AutoOrbitReward);
        Assert.Empty(autoOrbitResponse.AutoOrbitReward.DropReward);
        Assert.Empty(userDb.EntityIUserQuestAutoOrbit);
    }

    [Fact]
    public async Task StartAndFinishEventQuest_AutoOrbitTracksEventChapter()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = new();
        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.StartEventQuest(
            new StartEventQuestRequest
            {
                EventQuestChapterId = EventQuestChapterId,
                QuestId = EventQuestId,
                MaxAutoOrbitCount = 3
            },
            FakeServerCallContext.For(UserId));

        FinishEventQuestResponse response = await service.FinishEventQuest(
            new FinishEventQuestRequest
            {
                EventQuestChapterId = EventQuestChapterId,
                QuestId = EventQuestId,
                IsAutoOrbit = true
            },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response.AutoOrbitReward);
        Assert.Empty(response.AutoOrbitReward.DropReward);

        EntityIUserQuestAutoOrbit autoOrbit = Assert.Single(userDb.EntityIUserQuestAutoOrbit);
        Assert.Equal(QuestType.EVENT_QUEST, autoOrbit.QuestType);
        Assert.Equal(EventQuestChapterId, autoOrbit.ChapterId);
        Assert.Equal(EventQuestId, autoOrbit.QuestId);
        Assert.Equal(3, autoOrbit.MaxAutoOrbitCount);
        Assert.Equal(1, autoOrbit.ClearedAutoOrbitCount);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task FinishEventQuest_OfADuskChapter_MarksTheQuestInLimitContentStatusOnlyOnAWin(bool isRetired, bool marked)
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        masterDb.EntityMEventQuestChapter.Add(new EntityMEventQuestChapter
        {
            EventQuestChapterId = EventQuestChapterId,
            EventQuestType = EventQuestType.LIMIT_CONTENT
        });
        DarkUserMemoryDatabase userDb = new();
        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.StartEventQuest(
            new StartEventQuestRequest { EventQuestChapterId = EventQuestChapterId, QuestId = EventQuestId },
            FakeServerCallContext.For(UserId));
        await service.FinishEventQuest(
            new FinishEventQuestRequest { EventQuestChapterId = EventQuestChapterId, QuestId = EventQuestId, IsRetired = isRetired },
            FakeServerCallContext.For(UserId));

        if (marked)
        {
            EntityIUserQuestLimitContentStatus status = Assert.Single(userDb.EntityIUserQuestLimitContentStatus);
            Assert.Equal(EventQuestId, status.QuestId);
            Assert.Equal(EventQuestChapterId, status.EventQuestChapterId);
            Assert.Equal(1, status.LimitContentQuestStatusType);
        }
        else
        {
            Assert.Empty(userDb.EntityIUserQuestLimitContentStatus);
        }
    }

    [Fact]
    public async Task ReceiveTowerAccumulationReward_PaysOnlyTiersTheClearedMissionsReach()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        masterDb.EntityMEventQuestChapter.Add(new EntityMEventQuestChapter { EventQuestChapterId = TowerChapterId, EventQuestType = EventQuestType.TOWER, EventQuestSequenceGroupId = 3010 });
        masterDb.EntityMEventQuestSequenceGroup.AddRange(
        [
            new EntityMEventQuestSequenceGroup { EventQuestSequenceGroupId = 3010, DifficultyType = DifficultyType.NORMAL, EventQuestSequenceId = 3011 },
            new EntityMEventQuestSequenceGroup { EventQuestSequenceGroupId = 3010, DifficultyType = DifficultyType.HARD, EventQuestSequenceId = 3012 }
        ]);
        masterDb.EntityMEventQuestSequence.AddRange(
        [
            new EntityMEventQuestSequence { EventQuestSequenceId = 3011, SortOrder = 1, QuestId = 30101 },
            new EntityMEventQuestSequence { EventQuestSequenceId = 3012, SortOrder = 1, QuestId = 30201 }
        ]);
        masterDb.EntityMQuestMission.Add(new EntityMQuestMission { QuestMissionId = 99, QuestMissionConditionType = QuestMissionConditionType.COMPLETE });
        DarkUserMemoryDatabase userDb = new();
        QuestServiceImpl service = CreateService(masterDb, userDb);
        void ClearMissions(int questId, params int[] missionIds)
        {
            foreach (int missionId in missionIds)
            {
                userDb.EntityIUserQuestMission.Add(new EntityIUserQuestMission { QuestId = questId, QuestMissionId = missionId, IsClear = true });
            }
        }

        ClearMissions(30101, 1, 2, 99);
        ClearMissions(99999, 1, 2, 3);
        await service.ReceiveTowerAccumulationReward(
            new ReceiveTowerAccumulationRewardRequest { EventQuestChapterId = TowerChapterId, TargetMissionClearCount = 5 },
            FakeServerCallContext.For(UserId));
        Assert.Equal(["701:1"], userDb.EntityIUserMaterial.Select(m => $"{m.MaterialId}:{m.Count}").ToArray());

        ClearMissions(30201, 1, 2, 3);
        await service.ReceiveTowerAccumulationReward(
            new ReceiveTowerAccumulationRewardRequest { EventQuestChapterId = TowerChapterId, TargetMissionClearCount = 5 },
            FakeServerCallContext.For(UserId));
        await service.ReceiveTowerAccumulationReward(
            new ReceiveTowerAccumulationRewardRequest { EventQuestChapterId = TowerChapterId, TargetMissionClearCount = 5 },
            FakeServerCallContext.For(UserId));

        EntityIUserEventQuestTowerAccumulationReward state = Assert.Single(userDb.EntityIUserEventQuestTowerAccumulationReward);
        Assert.Equal(5, state.LatestRewardReceiveQuestMissionClearCount);
        Assert.Equal(
            ["701:1", "702:2"],
            userDb.EntityIUserMaterial.OrderBy(m => m.MaterialId).Select(m => $"{m.MaterialId}:{m.Count}").ToArray());
    }

    [Fact]
    public async Task StartGuerrillaFreeOpen_FirstOpenOfTheDayIsFree_TheNextCostsGems_AndANewDayStartsOver()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMEventQuestGuerrillaFreeOpen.Add(new EntityMEventQuestGuerrillaFreeOpen { EventQuestGuerrillaFreeOpenId = 1, OpenMinutes = 60, DailyOpenMaxCount = 1, StartDatetime = 0, EndDatetime = long.MaxValue });
        masterDb.EntityMConfig.Add(new EntityMConfig { ConfigKey = "NECESSARY_GEM_COUNT_FOR_GUERRILLA_OPEN", Value = "100" });
        DarkUserMemoryDatabase userDb = new();
        userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = UserId, FreeGem = 30, PaidGem = 200 });
        QuestServiceImpl service = CreateService(masterDb, userDb);

        await service.StartGuerrillaFreeOpen(new Google.Protobuf.WellKnownTypes.Empty(), FakeServerCallContext.For(UserId));
        EntityIUserEventQuestGuerrillaFreeOpen open = Assert.Single(userDb.EntityIUserEventQuestGuerrillaFreeOpen);
        Assert.Equal((1, 60), (open.DailyOpenedCount, open.OpenMinutes));
        Assert.Equal((30, 200), (userDb.EntityIUserGem[0].FreeGem, userDb.EntityIUserGem[0].PaidGem));

        await service.StartGuerrillaFreeOpen(new Google.Protobuf.WellKnownTypes.Empty(), FakeServerCallContext.For(UserId));
        Assert.Equal(2, open.DailyOpenedCount);
        Assert.Equal((0, 130), (userDb.EntityIUserGem[0].FreeGem, userDb.EntityIUserGem[0].PaidGem));

        userDb.EntityIUserGem[0].PaidGem = 50;
        await Assert.ThrowsAsync<Grpc.Core.RpcException>(() => service.StartGuerrillaFreeOpen(new Google.Protobuf.WellKnownTypes.Empty(), FakeServerCallContext.For(UserId)));
        Assert.Equal(2, open.DailyOpenedCount);

        open.StartDatetime = GameDayHelper.StartOfDay(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) - 1;
        await service.StartGuerrillaFreeOpen(new Google.Protobuf.WellKnownTypes.Empty(), FakeServerCallContext.For(UserId));
        Assert.Equal(1, open.DailyOpenedCount);
        Assert.Equal(50, userDb.EntityIUserGem[0].PaidGem);
    }

    private static QuestServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        MissionTracker missions = TestServices.Missions(masterDb);
        return new QuestServiceImpl(store, masterDb, TestServices.QuestEngine(masterDb, missions), new QuestCampaignHelper(masterDb), missions, TimeProvider.System);
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
            },
            new EntityMQuest
            {
                QuestId = ExtraQuestId,
                IsRunInTheBackground = false,
                IsCountedAsQuest = true
            },
            new EntityMQuest
            {
                QuestId = EventQuestId,
                IsRunInTheBackground = false,
                IsCountedAsQuest = true
            }
        ]);

        masterDb.EntityMQuestScene.AddRange(
        [
            new EntityMQuestScene { QuestSceneId = PlayableFirstSceneId, QuestId = PlayableQuestId, SortOrder = 1 },
            new EntityMQuestScene { QuestSceneId = PlayableLastSceneId, QuestId = PlayableQuestId, SortOrder = 2 },
            new EntityMQuestScene { QuestSceneId = BackgroundSceneId, QuestId = BackgroundQuestId, SortOrder = 1 },
            new EntityMQuestScene { QuestSceneId = ExtraFirstSceneId, QuestId = ExtraQuestId, SortOrder = 1 },
            new EntityMQuestScene { QuestSceneId = ExtraSecondSceneId, QuestId = ExtraQuestId, SortOrder = 2 },
            new EntityMQuestScene { QuestSceneId = EventBattleSceneId, QuestId = EventQuestId, SortOrder = 1, IsBattleOnlyTarget = true },
            new EntityMQuestScene { QuestSceneId = EventSecondSceneId, QuestId = EventQuestId, SortOrder = 2 }
        ]);

        masterDb.EntityMQuestSceneChoice.AddRange(
        [
            new EntityMQuestSceneChoice
            {
                MainFlowQuestSceneId = PlayableFirstSceneId,
                QuestFlowType = QuestFlowType.MAIN_FLOW,
                ChoiceNumber = 1,
                QuestSceneChoiceEffectId = MainFlowChoiceEffectId
            },
            new EntityMQuestSceneChoice
            {
                MainFlowQuestSceneId = PlayableFirstSceneId,
                QuestFlowType = QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW,
                ChoiceNumber = 2,
                QuestSceneChoiceEffectId = AnotherRouteChoiceEffectId
            },
            new EntityMQuestSceneChoice
            {
                MainFlowQuestSceneId = PlayableFirstSceneId,
                QuestFlowType = QuestFlowType.MAIN_FLOW,
                ChoiceNumber = 3,
                QuestSceneChoiceEffectId = SideEffectChoiceEffectId
            }
        ]);

        masterDb.EntityMQuestSceneChoiceEffect.AddRange(
        [
            new EntityMQuestSceneChoiceEffect
            {
                QuestSceneChoiceEffectId = MainFlowChoiceEffectId,
                QuestSceneChoiceGroupingId = ChoiceGroupingId
            },
            new EntityMQuestSceneChoiceEffect
            {
                QuestSceneChoiceEffectId = AnotherRouteChoiceEffectId,
                QuestSceneChoiceGroupingId = ChoiceGroupingId
            },
            new EntityMQuestSceneChoiceEffect
            {
                QuestSceneChoiceEffectId = SideEffectChoiceEffectId,
                QuestSceneChoiceGroupingId = ChoiceGroupingId,
                QuestSceneChoiceCostumeEffectGroupId = SideEffectCostumeEffectGroupId,
                QuestSceneChoiceWeaponEffectGroupId = SideEffectWeaponEffectGroupId
            }
        ]);

        masterDb.EntityMQuestSceneChoiceCostumeEffectGroup.Add(new EntityMQuestSceneChoiceCostumeEffectGroup
        {
            QuestSceneChoiceCostumeEffectGroupId = SideEffectCostumeEffectGroupId,
            SortOrder = 1,
            CostumeId = SideEffectCostumeId
        });
        masterDb.EntityMQuestSceneChoiceWeaponEffectGroup.Add(new EntityMQuestSceneChoiceWeaponEffectGroup
        {
            QuestSceneChoiceWeaponEffectGroupId = SideEffectWeaponEffectGroupId,
            SortOrder = 1,
            WeaponId = SideEffectWeaponId
        });

        masterDb.EntityMCostume.Add(new EntityMCostume
        {
            CostumeId = SideEffectCostumeId,
            CharacterId = SideEffectCharacterId
        });
        masterDb.EntityMWeapon.Add(new EntityMWeapon
        {
            WeaponId = SideEffectWeaponId,
            WeaponAbilityGroupId = SideEffectWeaponAbilityGroupId,
            WeaponSkillGroupId = SideEffectWeaponSkillGroupId
        });
        masterDb.EntityMWeaponAbilityGroup.Add(new EntityMWeaponAbilityGroup
        {
            WeaponAbilityGroupId = SideEffectWeaponAbilityGroupId,
            SlotNumber = 1
        });
        masterDb.EntityMWeaponSkillGroup.Add(new EntityMWeaponSkillGroup
        {
            WeaponSkillGroupId = SideEffectWeaponSkillGroupId,
            SlotNumber = 1
        });

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

        masterDb.EntityMEventQuestTowerAccumulationReward.Add(new EntityMEventQuestTowerAccumulationReward
        {
            EventQuestChapterId = TowerChapterId,
            EventQuestTowerAccumulationRewardGroupId = 801
        });
        masterDb.EntityMEventQuestTowerAccumulationRewardGroup.AddRange(
        [
            new EntityMEventQuestTowerAccumulationRewardGroup
            {
                EventQuestTowerAccumulationRewardGroupId = 801,
                QuestMissionClearCount = 2,
                EventQuestTowerRewardGroupId = 901
            },
            new EntityMEventQuestTowerAccumulationRewardGroup
            {
                EventQuestTowerAccumulationRewardGroupId = 801,
                QuestMissionClearCount = 5,
                EventQuestTowerRewardGroupId = 902
            }
        ]);
        masterDb.EntityMEventQuestTowerRewardGroup.AddRange(
        [
            new EntityMEventQuestTowerRewardGroup
            {
                EventQuestTowerRewardGroupId = 901,
                SortOrder = 1,
                PossessionType = PossessionType.MATERIAL,
                PossessionId = 701,
                Count = 1
            },
            new EntityMEventQuestTowerRewardGroup
            {
                EventQuestTowerRewardGroupId = 902,
                SortOrder = 1,
                PossessionType = PossessionType.MATERIAL,
                PossessionId = 702,
                Count = 2
            }
        ]);

        return masterDb;
    }
}
