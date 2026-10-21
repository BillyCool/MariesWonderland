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
    public async Task UpdateMissionProgress_WithoutExistingUser_CreatesUserDatabaseWithoutTouchingPanelMissions()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDbWithPanelMissions();
        UserDataStore store = CreateStore(masterDb);
        MissionServiceImpl service = CreateService(store, masterDb);

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
        // Webview panel missions aren't supported yet (their targets lived on the original server's web page).
        Assert.Empty(createdDb.EntityIUserWebviewPanelMission);
    }

    [Fact]
    public async Task ReceiveMissionRewardsById_ForMissionsNotCleared_LeavesStateUntouched()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDbWithPanelMissions();
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
        userDb.EntityIUserWebviewPanelMission.Add(new EntityIUserWebviewPanelMission
        {
            UserId = UserId,
            WebviewPanelMissionPageId = 101,
            RewardReceiveDatetime = 999,
            LatestVersion = 888
        });
        UserDataStore store = CreateStore(masterDb, userDb);
        MissionServiceImpl service = CreateService(store, masterDb);

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

        EntityIUserWebviewPanelMission panel = Assert.Single(userDb.EntityIUserWebviewPanelMission);
        Assert.Equal(101, panel.WebviewPanelMissionPageId);
        Assert.Equal(999, panel.RewardReceiveDatetime);
        Assert.Equal(888, panel.LatestVersion);
    }

    [Fact]
    public async Task ReceiveMissionPassRewards_ForAPassNotInMasterData_GrantsNothing()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDbWithPanelMissions();
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
        UserDataStore store = CreateStore(masterDb, userDb);
        MissionServiceImpl service = CreateService(store, masterDb);

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
        Assert.Empty(userDb.EntityIUserWebviewPanelMission);
    }

    private static MissionServiceImpl CreateService(UserDataStore store, DarkMasterMemoryDatabase masterDb) => new(store, masterDb, TestServices.Missions(masterDb), TimeProvider.System);

    private static UserDataStore CreateStore(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase? userDb = null)
    {
        UserDataStore store = new(TimeProvider.System);
        if (userDb != null)
            store.Set(UserId, userDb);

        return store;
    }

    private static DarkMasterMemoryDatabase CreateMasterDbWithPanelMissions()
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return new DarkMasterMemoryDatabase
        {
            EntityMWebviewPanelMissionPage =
            [
                new EntityMWebviewPanelMissionPage { WebviewPanelMissionPageId = 101 },
                new EntityMWebviewPanelMissionPage { WebviewPanelMissionPageId = 202 }
            ],
            EntityMWebviewPanelMission =
            [
                new EntityMWebviewPanelMission
                {
                    WebviewPanelMissionId = 1,
                    Page = 1,
                    WebviewPanelMissionPageId = 101,
                    StartDatetime = nowMs - 10_000,
                    EndDatetime = nowMs + 10_000
                },
                new EntityMWebviewPanelMission
                {
                    WebviewPanelMissionId = 1,
                    Page = 2,
                    WebviewPanelMissionPageId = 202,
                    StartDatetime = nowMs - 10_000,
                    EndDatetime = 0
                },
                new EntityMWebviewPanelMission
                {
                    WebviewPanelMissionId = 2,
                    Page = 1,
                    WebviewPanelMissionPageId = 303,
                    StartDatetime = nowMs - 10_000,
                    EndDatetime = nowMs - 1
                },
                new EntityMWebviewPanelMission
                {
                    WebviewPanelMissionId = 3,
                    Page = 1,
                    WebviewPanelMissionPageId = 404,
                    StartDatetime = nowMs - 10_000,
                    EndDatetime = nowMs + 10_000
                }
            ]
        };
    }
}
