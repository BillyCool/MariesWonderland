using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Notification;
using MariesWonderland.Tests.Infrastructure;
using NotificationServiceImpl = MariesWonderland.Services.NotificationService;

namespace MariesWonderland.Tests.Services;

public class NotificationServiceTests
{
    private const long UserId = 1L;

    [Fact]
    public async Task GetHeaderNotification_CountsOnlyCurrentUsersUnreceivedGifts()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntitySUserGift.AddRange(
        [
            CreateGift("pending-first", receivedDatetime: 0),
            CreateGift("pending-second", receivedDatetime: 0),
            CreateGift("received", receivedDatetime: 1234),
            CreateGift("other-user", userId: 999L, receivedDatetime: 0)
        ]);

        NotificationServiceImpl service = CreateService(userDb);

        GetHeaderNotificationResponse response = await service.GetHeaderNotification(
            new Empty(),
            FakeServerCallContext.For(UserId));

        Assert.Equal(2, response.GiftNotReceiveCount);
        Assert.Equal(0, response.FriendRequestReceiveCount);
        Assert.False(response.IsExistUnreadInformation);
    }

    [Fact]
    public async Task GetHeaderNotification_ForNewUser_ReturnsZeroCountsAndCreatesDatabase()
    {
        UserDataStore store = new(TimeProvider.System);
        NotificationServiceImpl service = new(store, TimeProvider.System);

        GetHeaderNotificationResponse response = await service.GetHeaderNotification(
            new Empty(),
            FakeServerCallContext.For(UserId));

        Assert.Equal(0, response.GiftNotReceiveCount);
        Assert.Equal(0, response.FriendRequestReceiveCount);
        Assert.False(response.IsExistUnreadInformation);
        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? createdDb));
        Assert.NotNull(createdDb);
        Assert.Empty(createdDb.EntitySUserGift);
    }

    [Fact]
    public async Task GetHeaderNotification_WhenAllGiftsReceived_ReturnsZeroPendingGiftCount()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntitySUserGift.AddRange(
        [
            CreateGift("received-first", receivedDatetime: 111),
            CreateGift("received-second", receivedDatetime: 222)
        ]);

        NotificationServiceImpl service = CreateService(userDb);

        GetHeaderNotificationResponse response = await service.GetHeaderNotification(
            new Empty(),
            FakeServerCallContext.For(UserId));

        Assert.Equal(0, response.GiftNotReceiveCount);
        Assert.Equal(0, response.FriendRequestReceiveCount);
        Assert.False(response.IsExistUnreadInformation);
    }

    private static NotificationServiceImpl CreateService(DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return new NotificationServiceImpl(store, TimeProvider.System);
    }

    private static EntitySUserGift CreateGift(string userGiftUuid, long userId = UserId, long receivedDatetime = 0)
        => new()
        {
            UserId = userId,
            UserGiftUuid = userGiftUuid,
            ReceivedDatetime = receivedDatetime
        };
}
