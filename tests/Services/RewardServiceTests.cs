using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
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
        Assert.Equal(
            ["12:0:5", "5:400:2"],
            response.WeeklyScoreReward.Select(RewardKey).ToArray());

        EntityIUserGem gem = Assert.Single(userDb.EntityIUserGem);
        Assert.Equal(15, gem.FreeGem);
        Assert.Equal(20, gem.PaidGem);

        EntityIUserMaterial material = Assert.Single(userDb.EntityIUserMaterial);
        Assert.Equal(MaterialId, material.MaterialId);
        Assert.Equal(3, material.Count);
        Assert.Equal(123, material.FirstAcquisitionDatetime);

        EntityIUserBigHuntWeeklyStatus status = Assert.Single(userDb.EntityIUserBigHuntWeeklyStatus);
        Assert.Equal(weeklyVersion, status.BigHuntWeeklyVersion);
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

        EntityIUserBigHuntWeeklyStatus status = Assert.Single(userDb.EntityIUserBigHuntWeeklyStatus);
        Assert.Equal(weeklyVersion, status.BigHuntWeeklyVersion);
        Assert.True(status.IsReceivedWeeklyReward);
    }

    [Fact]
    public async Task ReceiveBigHuntReward_ForFreshUserWithoutScores_CreatesStatusAndReturnsZeroedResults()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long weeklyVersion = GetWeeklyVersion(nowMs);
        DarkMasterMemoryDatabase masterDb = CreateMasterDb(nowMs);
        UserDataStore store = new(masterDb);
        RewardServiceImpl service = new(store, masterDb);

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
        Assert.Equal(weeklyVersion, status.BigHuntWeeklyVersion);
        Assert.True(status.IsReceivedWeeklyReward);
    }

    [Fact]
    public async Task PlaceholderRewardEndpoints_ReturnEmptyResponsesWithoutCreatingUserData()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        DarkMasterMemoryDatabase masterDb = CreateMasterDb(nowMs);
        UserDataStore store = new(masterDb);
        RewardServiceImpl service = new(store, masterDb);

        ReceivePvpRewardResponse pvp = await service.ReceivePvpReward(new Empty(), FakeServerCallContext.For(UserId));
        ReceiveLabyrinthSeasonRewardResponse labyrinth = await service.ReceiveLabyrinthSeasonReward(new Empty(), FakeServerCallContext.For(UserId));
        ReceiveMissionPassRemainingRewardResponse missionPass = await service.ReceiveMissionPassRemainingReward(new Empty(), FakeServerCallContext.For(UserId));

        Assert.Equal(0, pvp.CalculateSize());
        Assert.Equal(0, labyrinth.CalculateSize());
        Assert.Equal(0, missionPass.CalculateSize());
        Assert.False(store.TryGet(UserId, out _));
    }

    private static RewardServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(masterDb);
        store.Set(UserId, userDb);
        return new RewardServiceImpl(store, masterDb);
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
        return masterDb;
    }

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static long GetWeeklyVersion(long millis)
    {
        DateTimeOffset dt = DateTimeOffset.FromUnixTimeMilliseconds(millis).ToUniversalTime();
        int weekday = (int)dt.DayOfWeek;
        if (weekday == 0)
        {
            weekday = 7;
        }

        DateTimeOffset monday = new(dt.Year, dt.Month, dt.Day, 0, 0, 0, TimeSpan.Zero);
        return monday.AddDays(-(weekday - 1)).ToUnixTimeMilliseconds();
    }

    private static string RewardKey(BigHuntReward reward)
        => $"{reward.PossessionType}:{reward.PossessionId}:{reward.Count}";
}
