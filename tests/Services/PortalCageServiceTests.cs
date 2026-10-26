using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.PortalCage;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Time.Testing;
using PortalCageServiceImpl = MariesWonderland.Services.PortalCageService;

namespace MariesWonderland.Tests.Services;

public class PortalCageServiceTests
{
    private const long UserId = 1L;
    private const long OtherUserId = 99L;
    private const int GoldId = 1;
    private const long HourMs = 3_600_000;
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly long NowMs = Now.ToUnixTimeMilliseconds();

    [Fact]
    public async Task UpdatePortalCageSceneProgress_ForNewUser_CreatesStatusAndMarksProgress()
    {
        UserDataStore store = new(TimeProvider.System);
        PortalCageServiceImpl service = new(store, new DarkMasterMemoryDatabase(), new GameConfig(), new FakeTimeProvider(Now));

        UpdatePortalCageSceneProgressResponse response = await service.UpdatePortalCageSceneProgress(
            new UpdatePortalCageSceneProgressRequest(),
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? userDb));
        Assert.NotNull(userDb);

        EntityIUserPortalCageStatus status = Assert.Single(userDb.EntityIUserPortalCageStatus);
        Assert.Equal(UserId, status.UserId);
        Assert.True(status.IsCurrentProgress);
        // The fruit timer starts the first time the user is in Mama's Room.
        Assert.Equal(NowMs, status.DropItemStartDatetime);
        Assert.Equal(0, status.CurrentDropItemCount);
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
        // The fruit count follows the timer: long past the start, the tree is full.
        Assert.Equal(3, status.CurrentDropItemCount);
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
    public async Task UpdatePortalCageSceneProgress_ClearsActiveSideStoryPointer()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserSideStoryQuestSceneProgressStatus.AddRange(
        [
            new EntityIUserSideStoryQuestSceneProgressStatus
            {
                UserId = UserId,
                CurrentSideStoryQuestId = 123,
                CurrentSideStoryQuestSceneId = 456
            },
            new EntityIUserSideStoryQuestSceneProgressStatus
            {
                UserId = OtherUserId,
                CurrentSideStoryQuestId = 789,
                CurrentSideStoryQuestSceneId = 987
            }
        ]);

        PortalCageServiceImpl service = CreateService(userDb);

        await service.UpdatePortalCageSceneProgress(
            new UpdatePortalCageSceneProgressRequest(),
            FakeServerCallContext.For(UserId));

        EntityIUserSideStoryQuestSceneProgressStatus userProgress = Assert.Single(
            userDb.EntityIUserSideStoryQuestSceneProgressStatus,
            entry => entry.UserId == UserId);
        Assert.Equal(0, userProgress.CurrentSideStoryQuestId);
        Assert.Equal(0, userProgress.CurrentSideStoryQuestSceneId);

        EntityIUserSideStoryQuestSceneProgressStatus otherUserProgress = Assert.Single(
            userDb.EntityIUserSideStoryQuestSceneProgressStatus,
            entry => entry.UserId == OtherUserId);
        Assert.Equal(789, otherUserProgress.CurrentSideStoryQuestId);
        Assert.Equal(987, otherUserProgress.CurrentSideStoryQuestSceneId);
    }

    [Fact]
    public async Task GetDropItem_ForANewUser_StartsTheTreeWithoutFruit()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        PortalCageServiceImpl service = CreateService(userDb);

        GetDropItemResponse response = await service.GetDropItem(new Empty(), FakeServerCallContext.For(UserId));

        Assert.Empty(response.PortalCageDropItem);
        EntityIUserPortalCageStatus status = Assert.Single(userDb.EntityIUserPortalCageStatus);
        Assert.Equal((NowMs, 0), (status.DropItemStartDatetime, status.CurrentDropItemCount));
    }

    [Fact]
    public async Task EnteringMamasRoom_ShowsTheFruitThatFell_UpToThree()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserPortalCageStatus.Add(new EntityIUserPortalCageStatus { UserId = UserId, DropItemStartDatetime = NowMs - 17 * HourMs });
        PortalCageServiceImpl service = CreateService(userDb);

        await service.UpdatePortalCageSceneProgress(new UpdatePortalCageSceneProgressRequest { PortalCageSceneId = 1 }, FakeServerCallContext.For(UserId));
        Assert.Equal(2, userDb.EntityIUserPortalCageStatus.Single().CurrentDropItemCount);

        userDb.EntityIUserPortalCageStatus.Single().DropItemStartDatetime = NowMs - 100 * HourMs;
        await service.UpdatePortalCageSceneProgress(new UpdatePortalCageSceneProgressRequest { PortalCageSceneId = 1 }, FakeServerCallContext.For(UserId));
        Assert.Equal(3, userDb.EntityIUserPortalCageStatus.Single().CurrentDropItemCount);
    }

    [Fact]
    public async Task GetDropItem_GivesOneRollPerFruit_AndRestartsTheTimer()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserPortalCageStatus.Add(new EntityIUserPortalCageStatus { UserId = UserId, IsCurrentProgress = true, DropItemStartDatetime = NowMs - 17 * HourMs });
        PortalCageServiceImpl service = CreateService(userDb);

        GetDropItemResponse response = await service.GetDropItem(new Empty(), FakeServerCallContext.For(UserId));

        PortalCageDropItem drop = Assert.Single(response.PortalCageDropItem);
        Assert.Equal(((int)PossessionType.CONSUMABLE_ITEM, GoldId, 2000), (drop.PossessionType, drop.PossessionId, drop.Count));
        Assert.Equal(2000, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        EntityIUserPortalCageStatus status = Assert.Single(userDb.EntityIUserPortalCageStatus);
        Assert.Equal((NowMs, 0, true), (status.DropItemStartDatetime, status.CurrentDropItemCount, status.IsCurrentProgress));
    }

    [Fact]
    public async Task UpdatePortalCageSceneProgress_SetsCurrentMainQuestFlowToMainFlow()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserMainQuestFlowStatus.Add(new EntityIUserMainQuestFlowStatus
        {
            UserId = UserId,
            CurrentQuestFlowType = QuestFlowType.REPLAY_FLOW,
            LatestVersion = 123
        });

        PortalCageServiceImpl service = CreateService(userDb);

        await service.UpdatePortalCageSceneProgress(
            new UpdatePortalCageSceneProgressRequest { PortalCageSceneId = 77 },
            FakeServerCallContext.For(UserId));

        EntityIUserMainQuestFlowStatus flowStatus = Assert.Single(userDb.EntityIUserMainQuestFlowStatus);
        Assert.Equal(QuestFlowType.MAIN_FLOW, flowStatus.CurrentQuestFlowType);
        Assert.Equal(123, flowStatus.LatestVersion);
    }

    private static PortalCageServiceImpl CreateService(DarkUserMemoryDatabase userDb)
    {
        // The fruit timer of Mama's Room: one per 480 minutes, at most 3 (gimmick 81001001 in master data).
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMGimmick.Add(new EntityMGimmick { GimmickId = 81001001, GimmickType = GimmickType.MAP_ONLY_CAGE_INTERVAL_DROP_ITEM });
        masterDb.EntityMGimmickInterval.Add(new EntityMGimmickInterval { GimmickId = 81001001, MaxValue = 3, IntervalValue = 480 });
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return new PortalCageServiceImpl(store, masterDb, new GameConfig { ConsumableItemIdForGold = GoldId }, new FakeTimeProvider(Now));
    }
}
