using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Data;
using MariesWonderland.Proto.Mission;
using MariesWonderland.Tests.Infrastructure;
using MissionServiceImpl = MariesWonderland.Services.MissionService;

namespace MariesWonderland.Tests.Services;

public class MissionServiceTests
{
    private const long UserId = 1L;

    [Fact]
    public async Task UpdateMissionProgress_WithoutExistingUser_CreatesUserDatabaseAndReturnsEmptyDiff()
    {
        UserDataStore store = CreateStore();
        MissionServiceImpl service = CreateService(store);

        UpdateMissionProgressResponse response = await service.UpdateMissionProgress(
            new UpdateMissionProgressRequest
            {
                CageMeasurableValues = new CageMeasurableValues
                {
                    RunningDistanceMeters = 250,
                    MamaTappedCount = 3
                },
                PictureBookMeasurableValues = new PictureBookMeasurableValues
                {
                    DefeatWizardCount = 7,
                    RhythmInteractionMeasurableValues = new RhythmInteractionMeasurableValues
                    {
                        LiveTypeId = 9,
                        TapCount = 42
                    }
                }
            },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Empty(response.DiffUserData);
        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? createdDb));
        Assert.NotNull(createdDb);
        Assert.Equal(UserId, createdDb.UserId);
        Assert.Empty(createdDb.EntityIUserMission);
        Assert.Empty(createdDb.EntityIUserMissionCompletionProgress);
        Assert.Empty(createdDb.EntityIUserMissionPassPoint);
    }

    [Fact]
    public async Task ReceiveMissionRewardsById_WithExistingMissionState_LeavesStateUntouchedAndReturnsEmptyCollections()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserMission.Add(new EntityIUserMission
        {
            UserId = UserId,
            MissionId = 1001,
            StartDatetime = 1234,
            ProgressValue = 25,
            MissionProgressStatusType = MissionProgressStatusType.IN_PROGRESS,
            ClearDatetime = 5678,
            LatestVersion = 10
        });
        userDb.EntityIUserMissionCompletionProgress.Add(new EntityIUserMissionCompletionProgress
        {
            UserId = UserId,
            MissionId = 1001,
            ProgressValue = 25,
            LatestVersion = 11
        });
        UserDataStore store = CreateStore(userDb);
        MissionServiceImpl service = CreateService(store);

        ReceiveMissionRewardsResponse response = await service.ReceiveMissionRewardsById(
            new ReceiveMissionRewardsByIdRequest { MissionId = { 1001, 2002 } },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Empty(response.ReceivedPossession);
        Assert.Empty(response.ExpiredPossession);
        Assert.Empty(response.OverflowPossession);
        Assert.Empty(response.DiffUserData);

        EntityIUserMission mission = Assert.Single(userDb.EntityIUserMission);
        Assert.Equal(1001, mission.MissionId);
        Assert.Equal(25, mission.ProgressValue);
        Assert.Equal(MissionProgressStatusType.IN_PROGRESS, mission.MissionProgressStatusType);
        Assert.Equal(5678, mission.ClearDatetime);

        EntityIUserMissionCompletionProgress progress = Assert.Single(userDb.EntityIUserMissionCompletionProgress);
        Assert.Equal(1001, progress.MissionId);
        Assert.Equal(25, progress.ProgressValue);
    }

    [Fact]
    public async Task ReceiveMissionPassRewards_EachCallReturnsFreshEmptyResponseAndKeepsStoredPassProgress()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserMissionPassPoint.Add(new EntityIUserMissionPassPoint
        {
            UserId = UserId,
            MissionPassId = 55,
            Point = 900,
            PremiumRewardReceivedLevel = 4,
            NoPremiumRewardReceivedLevel = 6,
            LatestVersion = 12
        });
        UserDataStore store = CreateStore(userDb);
        MissionServiceImpl service = CreateService(store);

        ReceiveMissionPassRewardsResponse firstResponse = await service.ReceiveMissionPassRewards(
            new ReceiveMissionPassRewardsRequest { MissionPassId = 55 },
            FakeServerCallContext.For(UserId));
        firstResponse.ReceivedPossession.Add(new MissionPassReward { PossessionType = 1, PossessionId = 2, Count = 3 });
        firstResponse.OverflowPossession.Add(new MissionPassReward { PossessionType = 4, PossessionId = 5, Count = 6 });
        firstResponse.DiffUserData.Add("patched", new DiffData());

        ReceiveMissionPassRewardsResponse secondResponse = await service.ReceiveMissionPassRewards(
            new ReceiveMissionPassRewardsRequest { MissionPassId = 55 },
            FakeServerCallContext.For(UserId));

        Assert.NotSame(firstResponse, secondResponse);
        Assert.Empty(secondResponse.ReceivedPossession);
        Assert.Empty(secondResponse.OverflowPossession);
        Assert.Empty(secondResponse.DiffUserData);

        EntityIUserMissionPassPoint passPoint = Assert.Single(userDb.EntityIUserMissionPassPoint);
        Assert.Equal(55, passPoint.MissionPassId);
        Assert.Equal(900, passPoint.Point);
        Assert.Equal(4, passPoint.PremiumRewardReceivedLevel);
        Assert.Equal(6, passPoint.NoPremiumRewardReceivedLevel);
    }

    private static MissionServiceImpl CreateService(UserDataStore store) => new(store);

    private static UserDataStore CreateStore(DarkUserMemoryDatabase? userDb = null)
    {
        UserDataStore store = new(new DarkMasterMemoryDatabase());
        if (userDb != null)
            store.Set(UserId, userDb);

        return store;
    }
}
