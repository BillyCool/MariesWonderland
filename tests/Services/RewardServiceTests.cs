using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.BigHunt;
using MariesWonderland.Proto.Reward;
using MariesWonderland.Tests.Infrastructure;
using RewardServiceImpl = MariesWonderland.Services.RewardService;

namespace MariesWonderland.Tests.Services;

public class RewardServiceTests
{
    private const long UserId = 1L;
    private const int FireBossId = 100;
    private const int WaterBossId = 200;
    private const int MaterialId = 400;

    [Fact]
    public async Task ReceiveBigHuntReward_WhenPending_UsesLatestActiveScheduleAndGrantsEligibleRewards()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long weeklyVersion = GetWeeklyVersion(nowMs);
        DarkMasterMemoryDatabase masterDb = CreateMasterDb(nowMs);
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = UserId, FreeGem = 10, PaidGem = 20 });
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = MaterialId,
            Count = 1,
            FirstAcquisitionDatetime = 123
        });
        userDb.EntityIUserBigHuntWeeklyStatus.Add(new EntityIUserBigHuntWeeklyStatus
        {
            UserId = UserId,
            BigHuntWeeklyVersion = weeklyVersion,
            IsReceivedWeeklyReward = false
        });
        userDb.EntityIUserBigHuntWeeklyMaxScore.Add(new EntityIUserBigHuntWeeklyMaxScore
        {
            UserId = UserId,
            BigHuntWeeklyVersion = weeklyVersion,
            AttributeType = AttributeType.FIRE,
            MaxScore = 650
        });
        userDb.EntityIUserBigHuntWeeklyMaxScore.Add(new EntityIUserBigHuntWeeklyMaxScore
        {
            UserId = UserId,
            BigHuntWeeklyVersion = weeklyVersion,
            AttributeType = AttributeType.WATER,
            MaxScore = 90
        });

        RewardServiceImpl service = CreateService(masterDb, userDb);

        ReceiveBigHuntRewardResponse response = await service.ReceiveBigHuntReward(new Empty(), FakeServerCallContext.For(UserId));

        Assert.True(response.IsReceivedWeeklyScoreReward);
        Assert.Collection(
            response.WeeklyScoreResult,
            fire =>
            {
                Assert.Equal((int)AttributeType.FIRE, fire.AttributeType);
                Assert.Equal(650, fire.BeforeMaxScore);
                Assert.Equal(650, fire.CurrentMaxScore);
                Assert.Equal(650, fire.AfterMaxScore);
                Assert.Equal(22, fire.BeforeAssetGradeIconId);
                Assert.Equal(22, fire.CurrentAssetGradeIconId);
                Assert.Equal(22, fire.AfterAssetGradeIconId);
            },
            water =>
            {
                Assert.Equal((int)AttributeType.WATER, water.AttributeType);
                Assert.Equal(90, water.BeforeMaxScore);
                Assert.Equal(90, water.CurrentMaxScore);
                Assert.Equal(90, water.AfterMaxScore);
                Assert.Equal(0, water.BeforeAssetGradeIconId);
                Assert.Equal(0, water.CurrentAssetGradeIconId);
                Assert.Equal(0, water.AfterAssetGradeIconId);
            });
        // Per attribute, only the highest tier the weekly max reached: Fire's 650 gets the 500 tier, not the 100 one too.
        Assert.Equal(
            ["5:400:2"],
            response.WeeklyScoreReward.Select(RewardKey).ToArray());
        // The weekly result dialog shows this list, so it must carry the same rewards.
        Assert.Equal(
            ["5:400:2"],
            response.LastWeekWeeklyScoreReward.Select(RewardKey).ToArray());

        EntityIUserGem gem = Assert.Single(userDb.EntityIUserGem);
        Assert.Equal(10, gem.FreeGem);
        Assert.Equal(20, gem.PaidGem);

        EntityIUserMaterial material = Assert.Single(userDb.EntityIUserMaterial);
        Assert.Equal(MaterialId, material.MaterialId);
        Assert.Equal(3, material.Count);
        Assert.Equal(123, material.FirstAcquisitionDatetime);

        EntityIUserBigHuntWeeklyStatus status = Assert.Single(userDb.EntityIUserBigHuntWeeklyStatus, s => s.BigHuntWeeklyVersion == weeklyVersion);
        Assert.True(status.IsReceivedWeeklyReward);
    }

    [Fact]
    public async Task ReceiveBigHuntReward_WhenAlreadyReceived_DoesNotGrantRewardsAgain()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long weeklyVersion = GetWeeklyVersion(nowMs);
        DarkMasterMemoryDatabase masterDb = CreateMasterDb(nowMs);
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = UserId, FreeGem = 10, PaidGem = 20 });
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = MaterialId,
            Count = 1,
            FirstAcquisitionDatetime = 456
        });
        userDb.EntityIUserBigHuntWeeklyStatus.Add(new EntityIUserBigHuntWeeklyStatus
        {
            UserId = UserId,
            BigHuntWeeklyVersion = weeklyVersion,
            IsReceivedWeeklyReward = true
        });
        userDb.EntityIUserBigHuntWeeklyMaxScore.Add(new EntityIUserBigHuntWeeklyMaxScore
        {
            UserId = UserId,
            BigHuntWeeklyVersion = weeklyVersion,
            AttributeType = AttributeType.FIRE,
            MaxScore = 650
        });

        RewardServiceImpl service = CreateService(masterDb, userDb);

        ReceiveBigHuntRewardResponse response = await service.ReceiveBigHuntReward(new Empty(), FakeServerCallContext.For(UserId));

        Assert.True(response.IsReceivedWeeklyScoreReward);
        Assert.Empty(response.WeeklyScoreReward);
        Assert.Equal(
            [(int)AttributeType.FIRE, (int)AttributeType.WATER],
            response.WeeklyScoreResult.Select(result => result.AttributeType).ToArray());

        EntityIUserGem gem = Assert.Single(userDb.EntityIUserGem);
        Assert.Equal(10, gem.FreeGem);
        Assert.Equal(20, gem.PaidGem);

        EntityIUserMaterial material = Assert.Single(userDb.EntityIUserMaterial);
        Assert.Equal(1, material.Count);
        Assert.Equal(456, material.FirstAcquisitionDatetime);

        EntityIUserBigHuntWeeklyStatus status = Assert.Single(userDb.EntityIUserBigHuntWeeklyStatus, s => s.BigHuntWeeklyVersion == weeklyVersion);
        Assert.True(status.IsReceivedWeeklyReward);
    }

    [Fact]
    public async Task ReceiveBigHuntReward_ForAWeekWithoutASignIn_PaysNothing()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long weeklyVersion = GetWeeklyVersion(nowMs);
        DarkMasterMemoryDatabase masterDb = CreateMasterDb(nowMs);
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial { UserId = UserId, MaterialId = MaterialId, Count = 1 });
        userDb.EntityIUserBigHuntScheduleMaxScore.Add(new EntityIUserBigHuntScheduleMaxScore
        {
            UserId = UserId,
            BigHuntScheduleId = BigHuntRewardHelper.ResolveActiveScheduleId(masterDb, nowMs),
            BigHuntBossId = FireBossId,
            MaxScore = 650
        });
        RewardServiceImpl service = CreateService(masterDb, userDb);

        ReceiveBigHuntRewardResponse response = await service.ReceiveBigHuntReward(new Empty(), FakeServerCallContext.For(UserId));

        // The user has a season best but no row for last week: they didn't sign in during it.
        Assert.Empty(response.WeeklyScoreReward);
        Assert.Equal(1, Assert.Single(userDb.EntityIUserMaterial).Count);
        Assert.DoesNotContain(userDb.EntityIUserBigHuntWeeklyStatus, s => s.BigHuntWeeklyVersion == weeklyVersion);
    }

    [Fact]
    public async Task ReceiveBigHuntReward_ForFreshUserWithoutScores_StartsTheCurrentWeekAndReturnsZeroedResults()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long weeklyVersion = GetWeeklyVersion(nowMs);
        DarkMasterMemoryDatabase masterDb = CreateMasterDb(nowMs);
        UserDataStore store = new(TimeProvider.System);
        RewardServiceImpl service = new(store, masterDb, TimeProvider.System);

        ReceiveBigHuntRewardResponse response = await service.ReceiveBigHuntReward(new Empty(), FakeServerCallContext.For(UserId));

        Assert.True(response.IsReceivedWeeklyScoreReward);
        Assert.Empty(response.WeeklyScoreReward);
        Assert.Equal(2, response.WeeklyScoreResult.Count);
        Assert.All(response.WeeklyScoreResult, result =>
        {
            Assert.Equal(0, result.BeforeMaxScore);
            Assert.Equal(0, result.CurrentMaxScore);
            Assert.Equal(0, result.AfterMaxScore);
            Assert.Equal(0, result.BeforeAssetGradeIconId);
            Assert.Equal(0, result.CurrentAssetGradeIconId);
            Assert.Equal(0, result.AfterAssetGradeIconId);
        });

        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? createdDb));
        Assert.NotNull(createdDb);

        EntityIUserBigHuntWeeklyStatus status = Assert.Single(createdDb.EntityIUserBigHuntWeeklyStatus);
        Assert.Equal(GameDayHelper.StartOfWeek(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()), status.BigHuntWeeklyVersion);
        Assert.False(status.IsReceivedWeeklyReward);
    }

    [Fact]
    public async Task ReceivePvpReward_WhenWeeklyAndSeasonRewardsArePending_GrantsSupportedRewardsAndUpdatesStatus()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        DarkMasterMemoryDatabase masterDb = CreateMasterDb(nowMs);
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = UserId, FreeGem = 10, PaidGem = 1 });
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = MaterialId,
            Count = 1,
            FirstAcquisitionDatetime = 777
        });
        userDb.EntityIUserPvpStatus.Add(new EntityIUserPvpStatus
        {
            UserId = UserId,
            LatestRewardReceivePvpWeeklyVersion = 100,
            LatestRewardReceivePvpSeasonId = 0
        });
        userDb.EntityIUserPvpWeeklyResult.AddRange(
        [
            new EntityIUserPvpWeeklyResult
            {
                UserId = UserId,
                PvpWeeklyVersion = 101,
                PvpSeasonId = 1,
                FinalPoint = 250,
                FinalRank = 7
            },
            new EntityIUserPvpWeeklyResult
            {
                UserId = UserId,
                PvpWeeklyVersion = 102,
                PvpSeasonId = 2,
                FinalPoint = 450,
                FinalRank = 3
            }
        ]);

        // The Arena's record of season 1: six bots ahead, so the player finished 7th when it ended.
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        MariesWonderland.Arena.ArenaSeason seasonOne = new(1, nowMs - 20_000, nowMs - 10_000);
        store.Arena.Enter(seasonOne, UserId, 10, 100, null, nowMs - 15_000,
            () => Enumerable.Range(1, 6).Select(i => new MariesWonderland.Arena.ArenaEntry
            {
                UserId = MariesWonderland.Arena.ArenaBots.IdBase + i,
                PlayerId = MariesWonderland.Arena.ArenaBots.IdBase + i,
                Points = 1_000 + i,
            }), out _);
        RewardServiceImpl service = new(store, masterDb, TimeProvider.System);

        ReceivePvpRewardResponse response = await service.ReceivePvpReward(new Empty(), FakeServerCallContext.For(UserId));

        Assert.NotNull(response.WeeklyGradeResult);
        Assert.Equal(2, response.WeeklyGradeResult.TargetSeasonId);
        Assert.Equal(450, response.WeeklyGradeResult.PvpPoint);
        Assert.Equal(303, response.WeeklyGradeResult.PvpGradeWeeklyRewardGroupId);

        Assert.NotNull(response.WeeklyRankResult);
        Assert.Equal(2, response.WeeklyRankResult.TargetSeasonId);
        Assert.Equal(3, response.WeeklyRankResult.Rank);
        Assert.Equal(402, response.WeeklyRankResult.PvpWeeklyRankRewardGroupId);

        Assert.NotNull(response.SeasonRankResult);
        Assert.Equal(1, response.SeasonRankResult.TargetSeasonId);
        Assert.Equal(7, response.SeasonRankResult.Rank);
        Assert.Equal(501, response.SeasonRankResult.PvpSeasonRankRewardGroupId);

        // The rewards wait in the gift box (the result dialog says so); nothing is granted until they are received.
        EntityIUserGem gem = Assert.Single(userDb.EntityIUserGem);
        Assert.Equal(10, gem.FreeGem);
        Assert.Equal(1, gem.PaidGem);
        Assert.Equal(1, Assert.Single(userDb.EntityIUserMaterial).Count);

        Assert.Equal(5, userDb.EntitySUserGift.Count);
        Assert.All(userDb.EntitySUserGift, g => Assert.Equal(0, g.ReceivedDatetime));
        Assert.Equal(12, userDb.EntitySUserGift.Where(g => g.PossessionType == (int)PossessionType.FREE_GEM).Sum(g => g.Count));
        Assert.Equal(4, userDb.EntitySUserGift.Where(g => g.PossessionType == (int)PossessionType.PAID_GEM).Sum(g => g.Count));
        Assert.Equal(5, userDb.EntitySUserGift.Where(g => g.PossessionType == (int)PossessionType.MATERIAL && g.PossessionId == MaterialId).Sum(g => g.Count));

        EntityIUserPvpStatus status = Assert.Single(userDb.EntityIUserPvpStatus);
        Assert.Equal(102, status.LatestRewardReceivePvpWeeklyVersion);
        Assert.Equal(1, status.LatestRewardReceivePvpSeasonId);
    }

    [Fact]
    public async Task ReceivePvpReward_DoesNotPayTheWeekStillRunning()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long runningWeek = GameDayHelper.StartOfWeek(nowMs);
        DarkMasterMemoryDatabase masterDb = CreateMasterDb(nowMs);
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = UserId, FreeGem = 10, PaidGem = 1 });
        userDb.EntityIUserPvpStatus.Add(new EntityIUserPvpStatus { UserId = UserId, LatestRewardReceivePvpWeeklyVersion = 100 });
        userDb.EntityIUserPvpWeeklyResult.AddRange(
        [
            new EntityIUserPvpWeeklyResult { UserId = UserId, PvpWeeklyVersion = 101, PvpSeasonId = 1, FinalPoint = 250, FinalRank = 7 },
            new EntityIUserPvpWeeklyResult { UserId = UserId, PvpWeeklyVersion = runningWeek, PvpSeasonId = 2, FinalPoint = 450, FinalRank = 3 }
        ]);

        RewardServiceImpl service = CreateService(masterDb, userDb);

        ReceivePvpRewardResponse response = await service.ReceivePvpReward(new Empty(), FakeServerCallContext.For(UserId));

        Assert.Equal(1, response.WeeklyGradeResult.TargetSeasonId);
        Assert.Equal(101, Assert.Single(userDb.EntityIUserPvpStatus).LatestRewardReceivePvpWeeklyVersion);
    }

    [Fact]
    public async Task RewardEndpoints_WithoutStoredRewardState_ReturnEmptyResponsesAndGrantNothing()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        DarkMasterMemoryDatabase masterDb = CreateMasterDb(nowMs);
        UserDataStore store = new(TimeProvider.System);
        RewardServiceImpl service = new(store, masterDb, TimeProvider.System);

        ReceivePvpRewardResponse pvp = await service.ReceivePvpReward(new Empty(), FakeServerCallContext.For(UserId));
        ReceiveLabyrinthSeasonRewardResponse labyrinth = await service.ReceiveLabyrinthSeasonReward(new Empty(), FakeServerCallContext.For(UserId));
        ReceiveMissionPassRemainingRewardResponse missionPass = await service.ReceiveMissionPassRemainingReward(new Empty(), FakeServerCallContext.For(UserId));

        Assert.Equal(0, pvp.CalculateSize());
        Assert.Equal(0, labyrinth.CalculateSize());
        Assert.Equal(0, missionPass.CalculateSize());
        if (store.TryGet(UserId, out DarkUserMemoryDatabase? userDb))
        {
            Assert.Empty(userDb.EntityIUserMissionPassPoint);
            Assert.Empty(userDb.EntityIUserPvpStatus);
        }
    }

    private static RewardServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return new RewardServiceImpl(store, masterDb, TimeProvider.System);
    }

    private static DarkMasterMemoryDatabase CreateMasterDb(long nowMs)
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMBigHuntBoss.AddRange(
        [
            new EntityMBigHuntBoss
            {
                BigHuntBossId = FireBossId,
                BigHuntBossGradeGroupId = 10,
                AttributeType = AttributeType.FIRE
            },
            new EntityMBigHuntBoss
            {
                BigHuntBossId = WaterBossId,
                BigHuntBossGradeGroupId = 20,
                AttributeType = AttributeType.WATER
            }
        ]);
        masterDb.EntityMBigHuntBossGradeGroup.AddRange(
        [
            new EntityMBigHuntBossGradeGroup { BigHuntBossGradeGroupId = 10, NecessaryScore = 100, AssetGradeIconId = 11 },
            new EntityMBigHuntBossGradeGroup { BigHuntBossGradeGroupId = 10, NecessaryScore = 500, AssetGradeIconId = 22 },
            new EntityMBigHuntBossGradeGroup { BigHuntBossGradeGroupId = 20, NecessaryScore = 100, AssetGradeIconId = 33 }
        ]);
        masterDb.EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule.AddRange(
        [
            new EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule
            {
                BigHuntWeeklyAttributeScoreRewardGroupScheduleId = 1,
                AttributeType = AttributeType.FIRE,
                GroupIndex = 2,
                BigHuntScoreRewardGroupId = 100,
                StartDatetime = nowMs - 1_000
            },
            new EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule
            {
                BigHuntWeeklyAttributeScoreRewardGroupScheduleId = 1,
                AttributeType = AttributeType.FIRE,
                GroupIndex = 1,
                BigHuntScoreRewardGroupId = 101,
                StartDatetime = nowMs - 10_000
            }
        ]);
        masterDb.EntityMBigHuntScoreRewardGroup.AddRange(
        [
            new EntityMBigHuntScoreRewardGroup { BigHuntScoreRewardGroupId = 100, NecessaryScore = 100, BigHuntRewardGroupId = 1000 },
            new EntityMBigHuntScoreRewardGroup { BigHuntScoreRewardGroupId = 100, NecessaryScore = 500, BigHuntRewardGroupId = 1001 },
            new EntityMBigHuntScoreRewardGroup { BigHuntScoreRewardGroupId = 101, NecessaryScore = 100, BigHuntRewardGroupId = 1002 }
        ]);
        masterDb.EntityMBigHuntRewardGroup.AddRange(
        [
            new EntityMBigHuntRewardGroup
            {
                BigHuntRewardGroupId = 1000,
                SortOrder = 1,
                PossessionType = PossessionType.FREE_GEM,
                PossessionId = 0,
                Count = 5
            },
            new EntityMBigHuntRewardGroup
            {
                BigHuntRewardGroupId = 1001,
                SortOrder = 1,
                PossessionType = PossessionType.MATERIAL,
                PossessionId = MaterialId,
                Count = 2
            },
            new EntityMBigHuntRewardGroup
            {
                BigHuntRewardGroupId = 1002,
                SortOrder = 1,
                PossessionType = PossessionType.FREE_GEM,
                PossessionId = 0,
                Count = 99
            }
        ]);
        masterDb.EntityMPvpSeason.AddRange(
        [
            new EntityMPvpSeason
            {
                PvpSeasonId = 1,
                SeasonStartDatetime = nowMs - 20_000,
                SeasonEndDatetime = nowMs - 10_000,
                PvpWeeklyRankRewardRankGroupId = 100,
                PvpSeasonRankRewardRankGroupId = 200
            },
            new EntityMPvpSeason
            {
                PvpSeasonId = 2,
                SeasonStartDatetime = nowMs - 5_000,
                SeasonEndDatetime = nowMs + 20_000,
                PvpWeeklyRankRewardRankGroupId = 101,
                PvpSeasonRankRewardRankGroupId = 201
            }
        ]);
        masterDb.EntityMPvpSeasonGrade.AddRange(
        [
            new EntityMPvpSeasonGrade { PvpSeasonId = 1, PvpGradeId = 1, NecessaryPvpPoint = 0, PvpGradeWeeklyRewardGroupId = 300 },
            new EntityMPvpSeasonGrade { PvpSeasonId = 1, PvpGradeId = 2, NecessaryPvpPoint = 200, PvpGradeWeeklyRewardGroupId = 301 },
            new EntityMPvpSeasonGrade { PvpSeasonId = 2, PvpGradeId = 1, NecessaryPvpPoint = 0, PvpGradeWeeklyRewardGroupId = 302 },
            new EntityMPvpSeasonGrade { PvpSeasonId = 2, PvpGradeId = 2, NecessaryPvpPoint = 400, PvpGradeWeeklyRewardGroupId = 303 }
        ]);
        masterDb.EntityMPvpWeeklyRankRewardRankGroup.AddRange(
        [
            new EntityMPvpWeeklyRankRewardRankGroup { PvpWeeklyRankRewardRankGroupId = 100, RankLowerLimit = 1, PvpWeeklyRankRewardGroupId = 400 },
            new EntityMPvpWeeklyRankRewardRankGroup { PvpWeeklyRankRewardRankGroupId = 100, RankLowerLimit = 6, PvpWeeklyRankRewardGroupId = 401 },
            new EntityMPvpWeeklyRankRewardRankGroup { PvpWeeklyRankRewardRankGroupId = 101, RankLowerLimit = 1, PvpWeeklyRankRewardGroupId = 402 },
            new EntityMPvpWeeklyRankRewardRankGroup { PvpWeeklyRankRewardRankGroupId = 101, RankLowerLimit = 5, PvpWeeklyRankRewardGroupId = 403 }
        ]);
        masterDb.EntityMPvpSeasonRankRewardRankGroup.AddRange(
        [
            new EntityMPvpSeasonRankRewardRankGroup { PvpSeasonRankRewardRankGroupId = 200, RankLowerLimit = 1, PvpSeasonRankRewardGroupId = 500 },
            new EntityMPvpSeasonRankRewardRankGroup { PvpSeasonRankRewardRankGroupId = 200, RankLowerLimit = 6, PvpSeasonRankRewardGroupId = 501 },
            new EntityMPvpSeasonRankRewardRankGroup { PvpSeasonRankRewardRankGroupId = 201, RankLowerLimit = 1, PvpSeasonRankRewardGroupId = 502 }
        ]);
        masterDb.EntityMPvpGradeWeeklyRewardGroup.AddRange(
        [
            new EntityMPvpGradeWeeklyRewardGroup { PvpGradeWeeklyRewardGroupId = 301, PvpRewardId = 9001, SortOrder = 1 },
            new EntityMPvpGradeWeeklyRewardGroup { PvpGradeWeeklyRewardGroupId = 303, PvpRewardId = 9002, SortOrder = 1 }
        ]);
        masterDb.EntityMPvpWeeklyRankRewardGroup.AddRange(
        [
            new EntityMPvpWeeklyRankRewardGroup { PvpWeeklyRankRewardGroupId = 401, PvpRewardId = 9003, SortOrder = 1 },
            new EntityMPvpWeeklyRankRewardGroup { PvpWeeklyRankRewardGroupId = 402, PvpRewardId = 9004, SortOrder = 1 }
        ]);
        masterDb.EntityMPvpSeasonRankRewardGroup.AddRange(
        [
            new EntityMPvpSeasonRankRewardGroup { PvpSeasonRankRewardGroupId = 501, PvpRewardId = 9005, SortOrder = 1 }
        ]);
        masterDb.EntityMPvpReward.AddRange(
        [
            new EntityMPvpReward { PvpRewardId = 9001, PossessionType = PossessionType.FREE_GEM, PossessionId = 0, Count = 5 },
            new EntityMPvpReward { PvpRewardId = 9002, PossessionType = PossessionType.MATERIAL, PossessionId = MaterialId, Count = 2 },
            new EntityMPvpReward { PvpRewardId = 9003, PossessionType = PossessionType.PAID_GEM, PossessionId = 0, Count = 4 },
            new EntityMPvpReward { PvpRewardId = 9004, PossessionType = PossessionType.FREE_GEM, PossessionId = 0, Count = 7 },
            new EntityMPvpReward { PvpRewardId = 9005, PossessionType = PossessionType.MATERIAL, PossessionId = MaterialId, Count = 3 }
        ]);
        return masterDb;
    }

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    /// <summary>The game week (Monday 08:00 UTC) containing <paramref name="millis"/>.</summary>
    // The claimable week is the one that just ended.
    private static long GetWeeklyVersion(long millis) => BigHuntRewardHelper.ClaimWeekVersion(millis);

    private static string RewardKey(BigHuntReward reward)
        => $"{reward.PossessionType}:{reward.PossessionId}:{reward.Count}";
}
