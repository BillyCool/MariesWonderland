using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.PortalCage;
using MariesWonderland.Tests.Infrastructure;
using PortalCageServiceImpl = MariesWonderland.Services.PortalCageService;

namespace MariesWonderland.Tests.Services;

public class PortalCageServiceTests
{
    private const long UserId = 1L;
    private const long OtherUserId = 99L;

    [Fact]
    public async Task UpdatePortalCageSceneProgress_ForNewUser_CreatesStatusAndMarksProgress()
    {
        DarkMasterMemoryDatabase masterDb = new();
        UserDataStore store = new(masterDb);
        PortalCageServiceImpl service = new(store);

        UpdatePortalCageSceneProgressResponse response = await service.UpdatePortalCageSceneProgress(
            new UpdatePortalCageSceneProgressRequest(),
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? userDb));
        Assert.NotNull(userDb);

        EntityIUserPortalCageStatus status = Assert.Single(userDb.EntityIUserPortalCageStatus);
        Assert.Equal(UserId, status.UserId);
        Assert.True(status.IsCurrentProgress);
        Assert.Equal(0, status.DropItemStartDatetime);
        Assert.Equal(0, status.CurrentDropItemCount);
        Assert.Equal(0, status.LatestVersion);
    }

    [Fact]
    public async Task UpdatePortalCageSceneProgress_WithExistingStatus_ReusesRecordAndPreservesOtherFields()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserPortalCageStatus.AddRange(
        [
            new EntityIUserPortalCageStatus
            {
                UserId = UserId,
                IsCurrentProgress = false,
                DropItemStartDatetime = 1234,
                CurrentDropItemCount = 7,
                LatestVersion = 88
            },
            new EntityIUserPortalCageStatus
            {
                UserId = OtherUserId,
                IsCurrentProgress = false,
                DropItemStartDatetime = 4321,
                CurrentDropItemCount = 9,
                LatestVersion = 66
            }
        ]);

        PortalCageServiceImpl service = CreateService(userDb);

        await service.UpdatePortalCageSceneProgress(
            new UpdatePortalCageSceneProgressRequest(),
            FakeServerCallContext.For(UserId));

        Assert.Equal(2, userDb.EntityIUserPortalCageStatus.Count);

        EntityIUserPortalCageStatus status = Assert.Single(
            userDb.EntityIUserPortalCageStatus,
            entry => entry.UserId == UserId);
        Assert.True(status.IsCurrentProgress);
        Assert.Equal(1234, status.DropItemStartDatetime);
        Assert.Equal(7, status.CurrentDropItemCount);
        Assert.Equal(88, status.LatestVersion);

        EntityIUserPortalCageStatus otherUserStatus = Assert.Single(
            userDb.EntityIUserPortalCageStatus,
            entry => entry.UserId == OtherUserId);
        Assert.False(otherUserStatus.IsCurrentProgress);
        Assert.Equal(4321, otherUserStatus.DropItemStartDatetime);
        Assert.Equal(9, otherUserStatus.CurrentDropItemCount);
        Assert.Equal(66, otherUserStatus.LatestVersion);
    }

    [Fact]
    public async Task GetDropItem_ForNewUser_ReturnsEmptyResponseAndCreatesUserDatabase()
    {
        DarkMasterMemoryDatabase masterDb = new();
        UserDataStore store = new(masterDb);
        PortalCageServiceImpl service = new(store);

        GetDropItemResponse response = await service.GetDropItem(
            new Empty(),
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? userDb));
        Assert.NotNull(userDb);
        Assert.Equal(UserId, userDb.UserId);
        Assert.Empty(userDb.EntityIUserPortalCageStatus);
    }

    [Fact]
    public async Task GetDropItem_WithExistingStatus_DoesNotMutatePortalCageState()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserPortalCageStatus.Add(new EntityIUserPortalCageStatus
        {
            UserId = UserId,
            IsCurrentProgress = true,
            DropItemStartDatetime = 555,
            CurrentDropItemCount = 3,
            LatestVersion = 44
        });

        PortalCageServiceImpl service = CreateService(userDb);

        GetDropItemResponse response = await service.GetDropItem(
            new Empty(),
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);

        EntityIUserPortalCageStatus status = Assert.Single(userDb.EntityIUserPortalCageStatus);
        Assert.Equal(UserId, status.UserId);
        Assert.True(status.IsCurrentProgress);
        Assert.Equal(555, status.DropItemStartDatetime);
        Assert.Equal(3, status.CurrentDropItemCount);
        Assert.Equal(44, status.LatestVersion);
    }

    private static PortalCageServiceImpl CreateService(DarkUserMemoryDatabase userDb)
    {
        DarkMasterMemoryDatabase masterDb = new();
        UserDataStore store = new(masterDb);
        store.Set(UserId, userDb);
        return new PortalCageServiceImpl(store);
    }
}
