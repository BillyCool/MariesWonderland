using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Battle;
using MariesWonderland.Proto.BigHunt;
using MariesWonderland.Tests.Infrastructure;
using Grpc.Core;
using BigHuntService = MariesWonderland.Services.BigHuntService;

namespace MariesWonderland.Tests.Services;

public class BigHuntServiceTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private const long UserId = 1L;

    private BigHuntService CreateService(UserDataStore store) => Create(store, MasterDb);

    private static BigHuntService Create(UserDataStore store, DarkMasterMemoryDatabase masterDb)
    {
        MissionTracker missions = TestServices.Missions(masterDb);
        return new(store, masterDb, TestServices.QuestEngine(masterDb, missions), new QuestCampaignHelper(masterDb), new ImportantItemEffectHelper(masterDb), missions, TimeProvider.System);
    }

    [Fact]
    public async Task StartBigHuntQuest_WithQuestMissionGroup_InitializesQuestProgressAndStatus()
    {
        BigHuntScenario scenario = ResolveScenario();
        var userDb = CreateUserDb();
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);

        await service.StartBigHuntQuest(
            new StartBigHuntQuestRequest
            {
                BigHuntBossQuestId = scenario.BossQuest.BigHuntBossQuestId,
                BigHuntQuestId = scenario.HuntQuest.BigHuntQuestId,
                UserDeckNumber = 3,
                IsDryRun = true
            },
            ContextFor(UserId));

        EntityIUserBigHuntProgressStatus progress = Assert.Single(userDb.EntityIUserBigHuntProgressStatus);
        Assert.Equal(scenario.BossQuest.BigHuntBossQuestId, progress.CurrentBigHuntBossQuestId);
        Assert.Equal(scenario.HuntQuest.BigHuntQuestId, progress.CurrentBigHuntQuestId);
        Assert.Equal(0, progress.CurrentQuestSceneId);
        Assert.True(progress.IsDryRun);

        EntitySBigHuntSession session = Assert.Single(userDb.EntitySBigHuntSession);
        Assert.Equal(3, session.DeckNumber);

        EntityIUserBigHuntStatus status = Assert.Single(userDb.EntityIUserBigHuntStatus);
        Assert.Equal(scenario.BossQuest.BigHuntBossQuestId, status.BigHuntBossQuestId);
        Assert.Equal(1, status.DailyChallengeCount);
        Assert.True(status.LatestChallengeDatetime > 0);

        EntityIUserQuest userQuest = Assert.Single(userDb.EntityIUserQuest);
        Assert.Equal(scenario.MasterQuest.QuestId, userQuest.QuestId);
        Assert.Equal((int)QuestStateType.ACTIVE, userQuest.QuestStateType);
        Assert.True(userQuest.LatestStartDatetime > 0);

        List<int> expectedMissionIds = scenario.MasterQuest.QuestMissionGroupId == 0
            ? []
            : [.. MasterDb.EntityMQuestMissionGroup
                .Where(g => g.QuestMissionGroupId == scenario.MasterQuest.QuestMissionGroupId)
                .Select(g => g.QuestMissionId)
                .Distinct()
                .Order()];

        Assert.Equal(expectedMissionIds.Count, userDb.EntityIUserQuestMission.Count);
        Assert.Equal(expectedMissionIds, userDb.EntityIUserQuestMission.Select(m => m.QuestMissionId).Order().ToList());
    }

    [Fact]
    public async Task UpdateBigHuntQuestSceneProgress_WithoutProgress_CreatesAndStoresSceneId()
    {
        var userDb = CreateUserDb();
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);

        await service.UpdateBigHuntQuestSceneProgress(
            new UpdateBigHuntQuestSceneProgressRequest { QuestSceneId = 42 },
            ContextFor(UserId));

        EntityIUserBigHuntProgressStatus progress = Assert.Single(userDb.EntityIUserBigHuntProgressStatus);
        Assert.Equal(42, progress.CurrentQuestSceneId);
    }

    [Fact]
    public async Task SaveBigHuntBattleInfo_WithBattleDetail_PersistsBinaryAndAggregatesStats()
    {
        var userDb = CreateUserDb();
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);

        await service.SaveBigHuntBattleInfo(
            new SaveBigHuntBattleInfoRequest
            {
                BattleBinary = ByteString.CopyFrom([1, 2, 3, 4]),
                BigHuntBattleDetail = new BigHuntBattleDetail
                {
                    DeckType = 2,
                    UserTripleDeckNumber = 7,
                    BossKnockDownCount = 4,
                    MaxComboCount = 55,
                    CostumeBattleInfo =
                    {
                        new CostumeBattleInfo { TotalDamage = 125 },
                        new CostumeBattleInfo { TotalDamage = 375 }
                    }
                }
            },
            ContextFor(UserId));

        EntitySBigHuntSession session = Assert.Single(userDb.EntitySBigHuntSession);
        Assert.Equal([1, 2, 3, 4], session.BattleBinary);
        Assert.Equal(2, session.DeckType);
        Assert.Equal(7, session.UserTripleDeckNumber);
        Assert.Equal(4, session.BossKnockDownCount);
        Assert.Equal(55, session.MaxComboCount);
        Assert.Equal(500, session.TotalDamage);
    }

    [Fact]
    public async Task FinishBigHuntQuest_WhenRetired_ReturnsEmptyScoreAndClearsProgress()
    {
        BigHuntScenario scenario = ResolveScenario();
        var userDb = CreateUserDb();
        userDb.EntityIUserBigHuntProgressStatus.Add(new EntityIUserBigHuntProgressStatus
        {
            UserId = UserId,
            CurrentBigHuntBossQuestId = scenario.BossQuest.BigHuntBossQuestId,
            CurrentBigHuntQuestId = scenario.HuntQuest.BigHuntQuestId,
            CurrentQuestSceneId = 10
        });
        userDb.EntitySBigHuntSession.Add(new EntitySBigHuntSession
        {
            UserId = UserId,
            BattleBinary = [9, 8, 7],
            TotalDamage = 999
        });
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);

        FinishBigHuntQuestResponse response = await service.FinishBigHuntQuest(
            new FinishBigHuntQuestRequest
            {
                BigHuntBossQuestId = scenario.BossQuest.BigHuntBossQuestId,
                BigHuntQuestId = scenario.HuntQuest.BigHuntQuestId,
                IsRetired = true
            },
            ContextFor(UserId));

        Assert.Equal(0, response.ScoreInfo.UserScore);
        Assert.False(response.ScoreInfo.IsHighScore);
        Assert.Empty(response.ScoreReward);

        EntityIUserBigHuntProgressStatus progress = Assert.Single(userDb.EntityIUserBigHuntProgressStatus);
        Assert.Equal(0, progress.CurrentBigHuntBossQuestId);
        Assert.Equal(0, progress.CurrentBigHuntQuestId);
        Assert.Equal(0, progress.CurrentQuestSceneId);
        Assert.False(progress.IsDryRun);

        EntitySBigHuntSession session = Assert.Single(userDb.EntitySBigHuntSession);
        Assert.Empty(session.BattleBinary);
        Assert.Equal(0, session.TotalDamage);
    }

    [Fact]
    public async Task FinishBigHuntQuest_DryRunAppliesQuestCampaignBonusesToUnderlyingDrops()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMBigHuntQuest.Add(new EntityMBigHuntQuest
        {
            BigHuntQuestId = 100,
            QuestId = 200
        });
        masterDb.EntityMQuest.Add(new EntityMQuest
        {
            QuestId = 200,
            QuestPickupRewardGroupId = 300
        });
        masterDb.EntityMQuestPickupRewardGroup.Add(new EntityMQuestPickupRewardGroup
        {
            QuestPickupRewardGroupId = 300,
            BattleDropRewardId = 301
        });
        masterDb.EntityMBattleDropReward.Add(new EntityMBattleDropReward
        {
            BattleDropRewardId = 301,
            PossessionType = PossessionType.MATERIAL,
            PossessionId = 302,
            Count = 2
        });
        masterDb.EntityMQuestCampaignTargetGroup.Add(new EntityMQuestCampaignTargetGroup
        {
            QuestCampaignTargetGroupId = 303,
            QuestCampaignTargetIndex = 1,
            QuestCampaignTargetType = QuestCampaignTargetType.QUEST_TYPE,
            QuestCampaignTargetValue = (int)QuestType.BIG_HUNT_QUEST
        });
        masterDb.EntityMQuestCampaignEffectGroup.Add(new EntityMQuestCampaignEffectGroup
        {
            QuestCampaignEffectGroupId = 304,
            QuestCampaignEffectType = QuestCampaignEffectType.DROP_ITEM_ADD,
            QuestCampaignTargetItemGroupId = 305
        });
        masterDb.EntityMQuestCampaignTargetItemGroup.Add(new EntityMQuestCampaignTargetItemGroup
        {
            QuestCampaignTargetItemGroupId = 305,
            TargetIndex = 1,
            PossessionType = PossessionType.MATERIAL,
            PossessionId = 306,
            Count = 4
        });
        masterDb.EntityMQuestCampaign.Add(new EntityMQuestCampaign
        {
            QuestCampaignId = 307,
            QuestCampaignTargetGroupId = 303,
            QuestCampaignEffectGroupId = 304,
            StartDatetime = 0,
            EndDatetime = long.MaxValue,
            TargetUserStatusType = TargetUserStatusType.ALL
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = 200,
            ClearCount = 1
        });
        userDb.EntityIUserBigHuntProgressStatus.Add(new EntityIUserBigHuntProgressStatus
        {
            UserId = UserId,
            CurrentBigHuntQuestId = 100,
            IsDryRun = true
        });
        userDb.EntitySBigHuntSession.Add(new EntitySBigHuntSession
        {
            UserId = UserId
        });

        UserDataStore store = CreateStore(UserId, userDb);
        BigHuntService service = Create(store, masterDb);

        await service.FinishBigHuntQuest(
            new FinishBigHuntQuestRequest { BigHuntQuestId = 100 },
            ContextFor(UserId));

        Assert.Equal(
            ["302:2", "306:4"],
            userDb.EntityIUserMaterial.OrderBy(material => material.MaterialId).Select(material => $"{material.MaterialId}:{material.Count}").ToArray());
    }

    [Fact]
    public async Task FinishBigHuntQuest_NewHighScore_UpdatesScoresRewardsAndClearsBattleState()
    {
        BigHuntScenario scenario = ResolveScenario(requireScoreRewards: true);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int rewardGroupId = ResolveActiveScoreRewardGroupId(scenario.BossQuest.BigHuntScoreRewardGroupScheduleId, nowMs);
        long highestThreshold = MasterDb.EntityMBigHuntScoreRewardGroup
            .Where(t => t.BigHuntScoreRewardGroupId == rewardGroupId)
            .Select(t => t.NecessaryScore)
            .DefaultIfEmpty(0)
            .Max();
        long totalDamage = Math.Max(highestThreshold, 1000);
        int comboBonusPermil = 200;
        long expectedScore = CalculateScore(totalDamage, scenario.Coefficient.ScoreDifficultBonusPermil, comboBonusPermil);
        string[] expectedRewards = HighestTierRewards(rewardGroupId, expectedScore)
            .Select(r => RewardKey(r.Type, r.Id, r.Count))
            .ToArray();

        var userDb = CreateUserDb();
        SeedUserGem(userDb);
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = scenario.MasterQuest.QuestId,
            ClearCount = 1
        });
        userDb.EntityIUserBigHuntProgressStatus.Add(new EntityIUserBigHuntProgressStatus
        {
            UserId = UserId,
            CurrentBigHuntBossQuestId = scenario.BossQuest.BigHuntBossQuestId,
            CurrentBigHuntQuestId = scenario.HuntQuest.BigHuntQuestId
        });
        userDb.EntitySBigHuntSession.Add(new EntitySBigHuntSession
        {
            UserId = UserId,
            DeckNumber = 4,
            DeckType = 3,
            UserTripleDeckNumber = 6,
            BossKnockDownCount = 2,
            MaxComboCount = 55,
            TotalDamage = totalDamage,
            BattleBinary = [1, 3, 5]
        });
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);

        FinishBigHuntQuestResponse response = await service.FinishBigHuntQuest(
            new FinishBigHuntQuestRequest
            {
                BigHuntBossQuestId = scenario.BossQuest.BigHuntBossQuestId,
                BigHuntQuestId = scenario.HuntQuest.BigHuntQuestId
            },
            ContextFor(UserId));

        Assert.Equal(expectedScore, response.ScoreInfo.UserScore);
        Assert.True(response.ScoreInfo.IsHighScore);
        Assert.Equal(totalDamage, response.ScoreInfo.TotalDamage);
        Assert.Equal(totalDamage, response.ScoreInfo.BaseScore);
        Assert.Equal(scenario.Coefficient.ScoreDifficultBonusPermil, response.ScoreInfo.DifficultyBonusPermil);
        Assert.Equal(500, response.ScoreInfo.AliveBonusPermil);
        Assert.Equal(comboBonusPermil, response.ScoreInfo.MaxComboBonusPermil);
        Assert.Equal(ResolveGradeIconId(scenario.Boss.BigHuntBossId, expectedScore), response.ScoreInfo.AssetGradeIconId);
        Assert.Equal(expectedRewards, response.ScoreReward.Select(r => RewardKey((PossessionType)r.PossessionType, r.PossessionId, r.Count)).ToArray());

        EntityIUserBigHuntMaxScore maxScore = Assert.Single(userDb.EntityIUserBigHuntMaxScore);
        Assert.Equal(scenario.Boss.BigHuntBossId, maxScore.BigHuntBossId);
        Assert.Equal(expectedScore, maxScore.MaxScore);

        int activeScheduleId = ResolveActiveScheduleId(nowMs);
        EntityIUserBigHuntScheduleMaxScore scheduleMaxScore = Assert.Single(userDb.EntityIUserBigHuntScheduleMaxScore);
        Assert.Equal(activeScheduleId, scheduleMaxScore.BigHuntScheduleId);
        Assert.Equal(scenario.Boss.BigHuntBossId, scheduleMaxScore.BigHuntBossId);
        Assert.Equal(expectedScore, scheduleMaxScore.MaxScore);

        EntityIUserBigHuntWeeklyMaxScore weeklyMaxScore = Assert.Single(userDb.EntityIUserBigHuntWeeklyMaxScore);
        Assert.Equal(WeeklyVersion(nowMs), weeklyMaxScore.BigHuntWeeklyVersion);
        Assert.Equal(scenario.Boss.AttributeType, weeklyMaxScore.AttributeType);
        Assert.Equal(expectedScore, weeklyMaxScore.MaxScore);

        EntityIUserBigHuntProgressStatus progress = Assert.Single(userDb.EntityIUserBigHuntProgressStatus);
        Assert.Equal(0, progress.CurrentBigHuntBossQuestId);
        Assert.Equal(0, progress.CurrentBigHuntQuestId);
        Assert.Equal(0, progress.CurrentQuestSceneId);
        Assert.False(progress.IsDryRun);

        EntitySBigHuntSession session = Assert.Single(userDb.EntitySBigHuntSession);
        Assert.Empty(session.BattleBinary);
        Assert.Equal(0, session.TotalDamage);
        Assert.Equal(0, session.MaxComboCount);
        Assert.Equal(0, session.BossKnockDownCount);
        Assert.Equal(0, session.DeckType);
        Assert.Equal(0, session.UserTripleDeckNumber);
        Assert.Equal(4, session.DeckNumber);
    }

    [Fact]
    public async Task FinishBigHuntQuest_WithLowerScore_DoesNotOverwriteExistingMaxScores_ButStillPaysItsTier()
    {
        BigHuntScenario scenario = ResolveScenario();
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int rewardGroupId = ResolveActiveScoreRewardGroupId(scenario.BossQuest.BigHuntScoreRewardGroupScheduleId, nowMs);
        long weeklyVersion = WeeklyVersion(nowMs);
        int activeScheduleId = ResolveActiveScheduleId(nowMs);
        long existingHighScore = 50_000;
        long totalDamage = 10;
        long expectedScore = CalculateScore(totalDamage, scenario.Coefficient.ScoreDifficultBonusPermil, 0);

        var userDb = CreateUserDb();
        SeedUserGem(userDb);
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = scenario.MasterQuest.QuestId,
            ClearCount = 1
        });
        userDb.EntityIUserBigHuntProgressStatus.Add(new EntityIUserBigHuntProgressStatus
        {
            UserId = UserId,
            CurrentBigHuntBossQuestId = scenario.BossQuest.BigHuntBossQuestId,
            CurrentBigHuntQuestId = scenario.HuntQuest.BigHuntQuestId
        });
        userDb.EntitySBigHuntSession.Add(new EntitySBigHuntSession
        {
            UserId = UserId,
            TotalDamage = totalDamage,
            MaxComboCount = 0
        });
        userDb.EntityIUserBigHuntMaxScore.Add(new EntityIUserBigHuntMaxScore
        {
            UserId = UserId,
            BigHuntBossId = scenario.Boss.BigHuntBossId,
            MaxScore = existingHighScore
        });
        userDb.EntityIUserBigHuntScheduleMaxScore.Add(new EntityIUserBigHuntScheduleMaxScore
        {
            UserId = UserId,
            BigHuntScheduleId = activeScheduleId,
            BigHuntBossId = scenario.Boss.BigHuntBossId,
            MaxScore = existingHighScore
        });
        userDb.EntityIUserBigHuntWeeklyMaxScore.Add(new EntityIUserBigHuntWeeklyMaxScore
        {
            UserId = UserId,
            BigHuntWeeklyVersion = weeklyVersion,
            AttributeType = scenario.Boss.AttributeType,
            MaxScore = existingHighScore
        });
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);

        FinishBigHuntQuestResponse response = await service.FinishBigHuntQuest(
            new FinishBigHuntQuestRequest
            {
                BigHuntBossQuestId = scenario.BossQuest.BigHuntBossQuestId,
                BigHuntQuestId = scenario.HuntQuest.BigHuntQuestId
            },
            ContextFor(UserId));

        Assert.Equal(expectedScore, response.ScoreInfo.UserScore);
        Assert.False(response.ScoreInfo.IsHighScore);
        // Every finished run pays the highest tier its own score reached, high score or not.
        Assert.Equal(
            HighestTierRewards(rewardGroupId, expectedScore).Select(r => RewardKey(r.Type, r.Id, r.Count)).ToArray(),
            response.ScoreReward.Select(r => RewardKey((PossessionType)r.PossessionType, r.PossessionId, r.Count)).ToArray());
        Assert.Equal(existingHighScore, Assert.Single(userDb.EntityIUserBigHuntMaxScore).MaxScore);
        Assert.Equal(existingHighScore, Assert.Single(userDb.EntityIUserBigHuntScheduleMaxScore).MaxScore);
        Assert.Equal(existingHighScore, Assert.Single(userDb.EntityIUserBigHuntWeeklyMaxScore).MaxScore);
    }

    [Fact]
    public async Task RestartBigHuntQuest_WithSavedSession_ReturnsBattleBinaryAndIncrementsStatus()
    {
        BigHuntScenario scenario = ResolveScenario();
        var userDb = CreateUserDb();
        userDb.EntitySBigHuntSession.Add(new EntitySBigHuntSession
        {
            UserId = UserId,
            DeckNumber = 8,
            BattleBinary = [4, 2]
        });
        userDb.EntityIUserBigHuntProgressStatus.Add(new EntityIUserBigHuntProgressStatus
        {
            UserId = UserId,
            CurrentQuestSceneId = 77
        });
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);

        RestartBigHuntQuestResponse response = await service.RestartBigHuntQuest(
            new RestartBigHuntQuestRequest
            {
                BigHuntBossQuestId = scenario.BossQuest.BigHuntBossQuestId,
                BigHuntQuestId = scenario.HuntQuest.BigHuntQuestId
            },
            ContextFor(UserId));

        Assert.Equal([4, 2], response.BattleBinary.ToByteArray());
        Assert.Equal(8, response.DeckNumber);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserBigHuntProgressStatus).CurrentQuestSceneId);

        EntityIUserBigHuntStatus status = Assert.Single(userDb.EntityIUserBigHuntStatus);
        Assert.Equal(scenario.BossQuest.BigHuntBossQuestId, status.BigHuntBossQuestId);
        Assert.Equal(1, status.DailyChallengeCount);
        Assert.True(status.LatestChallengeDatetime > 0);
    }

    [Fact]
    public async Task SkipBigHuntQuest_WithoutExistingStatus_CreatesStatusAndAddsSkipCount()
    {
        BigHuntScenario scenario = ResolveScenario();
        var userDb = CreateUserDb();
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);

        await service.SkipBigHuntQuest(
            new SkipBigHuntQuestRequest
            {
                BigHuntBossQuestId = scenario.BossQuest.BigHuntBossQuestId,
                SkipCount = 3
            },
            ContextFor(UserId));

        EntityIUserBigHuntStatus status = Assert.Single(userDb.EntityIUserBigHuntStatus);
        Assert.Equal(scenario.BossQuest.BigHuntBossQuestId, status.BigHuntBossQuestId);
        Assert.Equal(3, status.DailyChallengeCount);
        Assert.True(status.LatestChallengeDatetime > 0);
    }

    [Fact]
    public async Task GetBigHuntTopData_WithSeededWeeklyScores_ReturnsExpectedResultsAndRewards()
    {
        BigHuntScenario scenario = ResolveScenario();
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long weeklyVersion = WeeklyVersion(nowMs);
        long lastWeekVersion = weeklyVersion - TimeSpan.FromDays(7).Ticks / TimeSpan.TicksPerMillisecond;
        long seededScore = 100_000;
        var userDb = CreateUserDb();
        userDb.EntityIUserBigHuntWeeklyStatus.Add(new EntityIUserBigHuntWeeklyStatus
        {
            UserId = UserId,
            BigHuntWeeklyVersion = lastWeekVersion,
            IsReceivedWeeklyReward = true
        });
        userDb.EntityIUserBigHuntWeeklyMaxScore.Add(new EntityIUserBigHuntWeeklyMaxScore
        {
            UserId = UserId,
            BigHuntWeeklyVersion = weeklyVersion,
            AttributeType = scenario.Boss.AttributeType,
            MaxScore = seededScore
        });
        userDb.EntityIUserBigHuntWeeklyMaxScore.Add(new EntityIUserBigHuntWeeklyMaxScore
        {
            UserId = UserId,
            BigHuntWeeklyVersion = lastWeekVersion,
            AttributeType = scenario.Boss.AttributeType,
            MaxScore = seededScore / 2
        });
        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);

        GetBigHuntTopDataResponse response = await service.GetBigHuntTopData(new Empty(), ContextFor(UserId));

        Assert.True(response.IsReceivedWeeklyScoreReward);
        Assert.Equal(MasterDb.EntityMBigHuntBoss.Count, response.WeeklyScoreResult.Count);
        Assert.All(response.WeeklyScoreResult, result =>
        {
            Assert.Equal(result.BeforeMaxScore, result.CurrentMaxScore);
            Assert.Equal(result.CurrentMaxScore, result.AfterMaxScore);
            Assert.Equal(result.BeforeAssetGradeIconId, result.CurrentAssetGradeIconId);
            Assert.Equal(result.CurrentAssetGradeIconId, result.AfterAssetGradeIconId);
        });

        string[] expectedResults = [.. MasterDb.EntityMBigHuntBoss.Select(boss =>
        {
            long maxScore = userDb.EntityIUserBigHuntWeeklyMaxScore
                .FirstOrDefault(m => m.UserId == UserId
                    && m.BigHuntWeeklyVersion == weeklyVersion
                    && m.AttributeType == boss.AttributeType)?.MaxScore ?? 0;
            int iconId = ResolveGradeIconId(boss.BigHuntBossId, maxScore);
            return $"{(int)boss.AttributeType}:{maxScore}:{iconId}";
        })];

        Assert.Equal(
            expectedResults,
            response.WeeklyScoreResult.Select(r => $"{r.AttributeType}:{r.CurrentMaxScore}:{r.CurrentAssetGradeIconId}").ToArray());

        Assert.Equal(
            ResolveWeeklyRewards(userDb, weeklyVersion, nowMs).Select(r => RewardKey((PossessionType)r.PossessionType, r.PossessionId, r.Count)).ToArray(),
            response.WeeklyScoreReward.Select(r => RewardKey((PossessionType)r.PossessionType, r.PossessionId, r.Count)).ToArray());

        Assert.Equal(
            ResolveWeeklyRewards(userDb, lastWeekVersion, nowMs).Select(r => RewardKey((PossessionType)r.PossessionType, r.PossessionId, r.Count)).ToArray(),
            response.LastWeekWeeklyScoreReward.Select(r => RewardKey((PossessionType)r.PossessionType, r.PossessionId, r.Count)).ToArray());
    }

    [Fact]
    public async Task FinishBigHuntQuest_WithSavedBattleReportDetails_ReturnsBattleReportWaves()
    {
        BigHuntScenario scenario = ResolveScenario();
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long weeklyVersion = WeeklyVersion(nowMs);
        int activeScheduleId = ResolveActiveScheduleId(nowMs);

        var userDb = CreateUserDb();
        userDb.EntityIUserBigHuntProgressStatus.Add(new EntityIUserBigHuntProgressStatus
        {
            UserId = UserId,
            CurrentBigHuntBossQuestId = scenario.BossQuest.BigHuntBossQuestId,
            CurrentBigHuntQuestId = scenario.HuntQuest.BigHuntQuestId
        });
        userDb.EntitySBigHuntSession.Add(new EntitySBigHuntSession
        {
            UserId = UserId,
            DeckNumber = 4
        });
        userDb.EntityIUserBigHuntMaxScore.Add(new EntityIUserBigHuntMaxScore
        {
            UserId = UserId,
            BigHuntBossId = scenario.Boss.BigHuntBossId,
            MaxScore = long.MaxValue / 4
        });
        userDb.EntityIUserBigHuntScheduleMaxScore.Add(new EntityIUserBigHuntScheduleMaxScore
        {
            UserId = UserId,
            BigHuntScheduleId = activeScheduleId,
            BigHuntBossId = scenario.Boss.BigHuntBossId,
            MaxScore = long.MaxValue / 4
        });
        userDb.EntityIUserBigHuntWeeklyMaxScore.Add(new EntityIUserBigHuntWeeklyMaxScore
        {
            UserId = UserId,
            BigHuntWeeklyVersion = weeklyVersion,
            AttributeType = scenario.Boss.AttributeType,
            MaxScore = long.MaxValue / 4
        });
        SeedDeckForBattleReport(userDb, DeckType.BIG_HUNT, 4, 501, 502);

        var store = CreateStore(UserId, userDb);
        var service = CreateService(store);

        await service.SaveBigHuntBattleInfo(
            new SaveBigHuntBattleInfoRequest
            {
                BattleBinary = ByteString.CopyFrom([1]),
                BigHuntBattleDetail = new BigHuntBattleDetail
                {
                    CostumeBattleInfo =
                    {
                        new CostumeBattleInfo
                        {
                            DeckCharacterNumber = 1,
                            TotalDamage = 125,
                            HitCount = 7,
                            BattleReportRandomDisplay = new BattleReportRandomDisplay
                            {
                                RandomDisplayValueType = (int)BigHuntBattleReportRandomDisplayType.MAX_DAMAGE,
                                RandomDisplayValue = 999
                            }
                        }
                    }
                }
            },
            ContextFor(UserId));

        await service.SaveBigHuntBattleInfo(
            new SaveBigHuntBattleInfoRequest
            {
                BattleBinary = ByteString.CopyFrom([2]),
                BigHuntBattleDetail = new BigHuntBattleDetail
                {
                    CostumeBattleInfo =
                    {
                        new CostumeBattleInfo
                        {
                            DeckCharacterNumber = 2,
                            TotalDamage = 75,
                            HitCount = 3,
                            BattleReportRandomDisplay = new BattleReportRandomDisplay
                            {
                                RandomDisplayValueType = (int)BigHuntBattleReportRandomDisplayType.CRITICAL_COUNT,
                                RandomDisplayValue = 12
                            }
                        }
                    }
                }
            },
            ContextFor(UserId));

        FinishBigHuntQuestResponse response = await service.FinishBigHuntQuest(
            new FinishBigHuntQuestRequest
            {
                BigHuntBossQuestId = scenario.BossQuest.BigHuntBossQuestId,
                BigHuntQuestId = scenario.HuntQuest.BigHuntQuestId
            },
            ContextFor(UserId));

        Assert.Equal(2, response.BattleReport.BattleReportWave.Count);
        Assert.Collection(
            response.BattleReport.BattleReportWave[0].BattleReportCostume,
            costume =>
            {
                Assert.Equal(501, costume.CostumeId);
                Assert.Equal(125, costume.TotalDamage);
                Assert.Equal(7, costume.HitCount);
                Assert.Equal((int)BigHuntBattleReportRandomDisplayType.MAX_DAMAGE, costume.BattleReportRandomDisplay.RandomDisplayValueType);
                Assert.Equal(999, costume.BattleReportRandomDisplay.RandomDisplayValue);
            });
        Assert.Collection(
            response.BattleReport.BattleReportWave[1].BattleReportCostume,
            costume =>
            {
                Assert.Equal(502, costume.CostumeId);
                Assert.Equal(75, costume.TotalDamage);
                Assert.Equal(3, costume.HitCount);
                Assert.Equal((int)BigHuntBattleReportRandomDisplayType.CRITICAL_COUNT, costume.BattleReportRandomDisplay.RandomDisplayValueType);
                Assert.Equal(12, costume.BattleReportRandomDisplay.RandomDisplayValue);
            });

        Assert.Empty(Assert.Single(userDb.EntitySBigHuntSession).BattleReportCostume);
    }

    private BigHuntScenario ResolveScenario(bool requireScoreRewards = false)
    {
        var huntQuestMatch = MasterDb.EntityMBigHuntQuest
            .Select(huntQuest => new
            {
                HuntQuest = huntQuest,
                MasterQuest = MasterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == huntQuest.QuestId),
                Coefficient = MasterDb.EntityMBigHuntQuestScoreCoefficient
                    .FirstOrDefault(c => c.BigHuntQuestScoreCoefficientId == huntQuest.BigHuntQuestScoreCoefficientId)
            })
            .FirstOrDefault(x =>
                x.MasterQuest is not null
                && x.Coefficient is not null
                && (x.MasterQuest.QuestMissionGroupId == 0
                    || MasterDb.EntityMQuestMissionGroup.Any(g => g.QuestMissionGroupId == x.MasterQuest.QuestMissionGroupId)))
            ?? throw new InvalidOperationException("No matching BigHunt quest scenario found in master data.");

        EntityMBigHuntBossQuest bossQuest = MasterDb.EntityMBigHuntBossQuest
            .FirstOrDefault(bq =>
                MasterDb.EntityMBigHuntBoss.Any(b => b.BigHuntBossId == bq.BigHuntBossId)
                && (!requireScoreRewards || HasScoreRewards(bq)))
            ?? throw new InvalidOperationException("No matching BigHunt boss quest scenario found in master data.");

        EntityMBigHuntBoss boss = MasterDb.EntityMBigHuntBoss
            .First(b => b.BigHuntBossId == bossQuest.BigHuntBossId);

        return new BigHuntScenario(huntQuestMatch.HuntQuest, huntQuestMatch.MasterQuest!, huntQuestMatch.Coefficient!, bossQuest, boss);
    }

    private bool HasScoreRewards(EntityMBigHuntBossQuest bossQuest)
    {
        HashSet<int> activeRewardGroupIds =
        [
            .. MasterDb.EntityMBigHuntScoreRewardGroupSchedule
                .Where(e => e.BigHuntScoreRewardGroupScheduleId == bossQuest.BigHuntScoreRewardGroupScheduleId)
                .Select(e => e.BigHuntScoreRewardGroupId)
        ];

        return MasterDb.EntityMBigHuntScoreRewardGroup
            .Where(t => activeRewardGroupIds.Contains(t.BigHuntScoreRewardGroupId))
            .Any(t => MasterDb.EntityMBigHuntRewardGroup.Any(r => r.BigHuntRewardGroupId == t.BigHuntRewardGroupId));
    }

    private static void SeedUserGem(DarkUserMemoryDatabase userDb, int freeGem = 0, int paidGem = 0)
        => userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = UserId, FreeGem = freeGem, PaidGem = paidGem });

    private static void SeedDeckForBattleReport(DarkUserMemoryDatabase userDb, DeckType deckType, int deckNumber, int costumeId01, int costumeId02)
    {
        userDb.EntityIUserDeck.Add(new EntityIUserDeck
        {
            UserId = UserId,
            DeckType = deckType,
            UserDeckNumber = deckNumber,
            UserDeckCharacterUuid01 = "dc-1",
            UserDeckCharacterUuid02 = "dc-2",
            Name = "Big Hunt Deck"
        });
        userDb.EntityIUserDeckCharacter.AddRange(
        [
            new EntityIUserDeckCharacter { UserId = UserId, UserDeckCharacterUuid = "dc-1", UserCostumeUuid = "costume-1" },
            new EntityIUserDeckCharacter { UserId = UserId, UserDeckCharacterUuid = "dc-2", UserCostumeUuid = "costume-2" }
        ]);
        userDb.EntityIUserCostume.AddRange(
        [
            new EntityIUserCostume { UserId = UserId, UserCostumeUuid = "costume-1", CostumeId = costumeId01 },
            new EntityIUserCostume { UserId = UserId, UserCostumeUuid = "costume-2", CostumeId = costumeId02 }
        ]);
    }

    private static long CalculateScore(long totalDamage, int difficultyBonusPermil, int comboBonusPermil)
        => totalDamage * (1000 + difficultyBonusPermil + 500 + comboBonusPermil) / 1000;

    private int ResolveActiveScheduleId(long nowMs)
    {
        EntityMBigHuntSchedule? active = MasterDb.EntityMBigHuntSchedule
            .FirstOrDefault(s => nowMs >= s.ChallengeStartDatetime && nowMs <= s.ChallengeEndDatetime);

        if (active is not null)
        {
            return active.BigHuntScheduleId;
        }

        return MasterDb.EntityMBigHuntSchedule
            .OrderByDescending(s => s.ChallengeEndDatetime)
            .Select(s => s.BigHuntScheduleId)
            .FirstOrDefault();
    }

    private int ResolveActiveScoreRewardGroupId(int scheduleId, long nowMs)
    {
        List<EntityMBigHuntScoreRewardGroupSchedule> entries = [.. MasterDb.EntityMBigHuntScoreRewardGroupSchedule
            .Where(e => e.BigHuntScoreRewardGroupScheduleId == scheduleId)
            .OrderByDescending(e => e.StartDatetime)];

        foreach (EntityMBigHuntScoreRewardGroupSchedule entry in entries)
        {
            if (nowMs >= entry.StartDatetime)
            {
                return entry.BigHuntScoreRewardGroupId;
            }
        }

        return entries.Count > 0 ? entries[^1].BigHuntScoreRewardGroupId : 0;
    }

    /// <summary>Each attribute has its own weekly reward schedule.</summary>
    private int ResolveActiveWeeklyRewardGroupId(AttributeType attributeType, long nowMs)
    {
        List<EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule> entries = [.. MasterDb.EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule
            .Where(e => e.AttributeType == attributeType)
            .OrderByDescending(e => e.StartDatetime)];

        foreach (EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule entry in entries)
        {
            if (nowMs >= entry.StartDatetime)
            {
                return entry.BigHuntScoreRewardGroupId;
            }
        }

        return entries.Count > 0 ? entries[^1].BigHuntScoreRewardGroupId : 0;
    }

    private int ResolveGradeIconId(int bossId, long score)
    {
        EntityMBigHuntBoss boss = MasterDb.EntityMBigHuntBoss.First(b => b.BigHuntBossId == bossId);
        int iconId = 0;

        foreach (EntityMBigHuntBossGradeGroup grade in MasterDb.EntityMBigHuntBossGradeGroup
            .Where(g => g.BigHuntBossGradeGroupId == boss.BigHuntBossGradeGroupId)
            .OrderBy(g => g.NecessaryScore))
        {
            if (score >= grade.NecessaryScore)
            {
                iconId = grade.AssetGradeIconId;
            }
            else
            {
                break;
            }
        }

        return iconId;
    }

    /// <summary>A score earns the rewards of the single highest tier it reaches (each tier is a full bundle).</summary>
    private List<(PossessionType Type, int Id, int Count)> HighestTierRewards(int scoreRewardGroupId, long score)
    {
        EntityMBigHuntScoreRewardGroup? tier = MasterDb.EntityMBigHuntScoreRewardGroup
            .Where(t => t.BigHuntScoreRewardGroupId == scoreRewardGroupId && t.NecessaryScore <= score)
            .OrderByDescending(t => t.NecessaryScore)
            .FirstOrDefault();
        return tier == null
            ? []
            : [.. MasterDb.EntityMBigHuntRewardGroup
                .Where(r => r.BigHuntRewardGroupId == tier.BigHuntRewardGroupId)
                .OrderBy(r => r.SortOrder)
                .Select(r => (r.PossessionType, r.PossessionId, r.Count))];
    }

    private List<BigHuntReward> ResolveWeeklyRewards(DarkUserMemoryDatabase userDb, long weeklyVersion, long nowMs)
    {
        List<BigHuntReward> rewards = [];

        foreach (EntityMBigHuntBoss boss in MasterDb.EntityMBigHuntBoss)
        {
            int rewardGroupId = ResolveActiveWeeklyRewardGroupId(boss.AttributeType, nowMs);
            if (rewardGroupId == 0)
            {
                continue;
            }

            long maxScore = userDb.EntityIUserBigHuntWeeklyMaxScore
                .FirstOrDefault(m => m.UserId == UserId
                    && m.BigHuntWeeklyVersion == weeklyVersion
                    && m.AttributeType == boss.AttributeType)?.MaxScore ?? 0;

            foreach ((PossessionType type, int id, int count) in HighestTierRewards(rewardGroupId, maxScore))
            {
                rewards.Add(new BigHuntReward
                {
                    PossessionType = (int)type,
                    PossessionId = id,
                    Count = count
                });
            }
        }

        return rewards;
    }

    /// <summary>The game week (Monday 08:00 UTC) containing <paramref name="millis"/>.</summary>
    private static long WeeklyVersion(long millis) => GameDayHelper.StartOfWeek(millis);

    private static string RewardKey(PossessionType type, int id, int count)
        => $"{(int)type}:{id}:{count}";

    private sealed record BigHuntScenario(
        EntityMBigHuntQuest HuntQuest,
        EntityMQuest MasterQuest,
        EntityMBigHuntQuestScoreCoefficient Coefficient,
        EntityMBigHuntBossQuest BossQuest,
        EntityMBigHuntBoss Boss);
}
