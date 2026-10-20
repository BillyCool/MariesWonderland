using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Gift;
using MariesWonderland.Tests.Infrastructure;
using GiftServiceImpl = MariesWonderland.Services.GiftService;

namespace MariesWonderland.Tests.Services;

public class GiftServiceTests
{
    private const long UserId = 1L;

    [Fact]
    public async Task ReceiveGift_MarksOnlyRequestedPendingGiftsForCurrentUser()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntitySUserGift.AddRange(
        [
            CreateGift("claim-me", expirationDatetime: 300, receivedDatetime: 0),
            CreateGift("already-claimed", expirationDatetime: 200, receivedDatetime: 12345),
            CreateGift("leave-alone", expirationDatetime: 100, receivedDatetime: 0),
            CreateGift("other-user", userId: 999L, expirationDatetime: 400, receivedDatetime: 0)
        ]);

        GiftServiceImpl service = CreateService(userDb);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        ReceiveGiftResponse response = await service.ReceiveGift(
            new ReceiveGiftRequest
            {
                UserGiftUuid = { "claim-me", "already-claimed", "other-user", "missing", "claim-me" }
            },
            FakeServerCallContext.For(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.Equal(["claim-me"], response.ReceivedGiftUuid);
        Assert.Empty(response.ExpiredGiftUuid);
        Assert.Empty(response.OverflowGiftUuid);

        EntitySUserGift claimedGift = Assert.Single(userDb.EntitySUserGift, gift => gift.UserGiftUuid == "claim-me");
        Assert.InRange(claimedGift.ReceivedDatetime, beforeMs, afterMs);

        EntitySUserGift alreadyClaimedGift = Assert.Single(userDb.EntitySUserGift, gift => gift.UserGiftUuid == "already-claimed");
        Assert.Equal(12345, alreadyClaimedGift.ReceivedDatetime);

        EntitySUserGift untouchedGift = Assert.Single(userDb.EntitySUserGift, gift => gift.UserGiftUuid == "leave-alone");
        Assert.Equal(0, untouchedGift.ReceivedDatetime);

        EntitySUserGift otherUserGift = Assert.Single(userDb.EntitySUserGift, gift => gift.UserGiftUuid == "other-user");
        Assert.Equal(0, otherUserGift.ReceivedDatetime);
    }

    [Fact]
    public async Task GetGiftList_ReturnsUnreceivedGiftsSortedPagedAndMapped()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntitySUserGift.AddRange(
        [
            CreateGift(
                "latest",
                possessionType: 30,
                possessionId: 300,
                count: 3,
                grantDatetime: 3000,
                expirationDatetime: 30000,
                descriptionGiftTextId: 3000,
                equipmentData: [9, 8]),
            CreateGift(
                "first",
                possessionType: 10,
                possessionId: 100,
                count: 1,
                grantDatetime: 1000,
                expirationDatetime: 10000,
                descriptionGiftTextId: 1000),
            CreateGift(
                "middle",
                possessionType: 20,
                possessionId: 200,
                count: 2,
                grantDatetime: 2000,
                expirationDatetime: 20000,
                descriptionGiftTextId: 2000,
                equipmentData: [1, 2, 3]),
            CreateGift("received", expirationDatetime: 40000, receivedDatetime: 444),
            CreateGift("other-user", userId: 999L, expirationDatetime: 5000, receivedDatetime: 0)
        ]);

        GiftServiceImpl service = CreateService(userDb);

        GetGiftListResponse response = await service.GetGiftList(
            new GetGiftListRequest
            {
                IsAscendingSort = true,
                GetCount = 2
            },
            FakeServerCallContext.For(UserId));

        Assert.Equal(2, response.TotalPageCount);
        Assert.Equal(0, response.NextCursor);
        Assert.Equal(0, response.PreviousCursor);
        Assert.Equal(2, response.Gift.Count);

        NotReceivedGift firstGift = response.Gift[0];
        Assert.Equal("first", firstGift.UserGiftUuid);
        Assert.Equal(10, firstGift.GiftCommon.PossessionType);
        Assert.Equal(100, firstGift.GiftCommon.PossessionId);
        Assert.Equal(1, firstGift.GiftCommon.Count);
        Assert.Equal(1000, firstGift.GiftCommon.DescriptionGiftTextId);
        Assert.Equal(1000, firstGift.GiftCommon.GrantDatetime.ToDateTimeOffset().ToUnixTimeMilliseconds());
        Assert.Equal(10000, firstGift.ExpirationDatetime.ToDateTimeOffset().ToUnixTimeMilliseconds());
        Assert.Empty(firstGift.GiftCommon.EquipmentData.ToByteArray());

        NotReceivedGift secondGift = response.Gift[1];
        Assert.Equal("middle", secondGift.UserGiftUuid);
        Assert.Equal(20, secondGift.GiftCommon.PossessionType);
        Assert.Equal(200, secondGift.GiftCommon.PossessionId);
        Assert.Equal(2, secondGift.GiftCommon.Count);
        Assert.Equal(2000, secondGift.GiftCommon.DescriptionGiftTextId);
        Assert.Equal(2000, secondGift.GiftCommon.GrantDatetime.ToDateTimeOffset().ToUnixTimeMilliseconds());
        Assert.Equal(20000, secondGift.ExpirationDatetime.ToDateTimeOffset().ToUnixTimeMilliseconds());
        Assert.Equal([1, 2, 3], secondGift.GiftCommon.EquipmentData.ToByteArray());
    }

    [Fact]
    public async Task GetGiftList_ForNewUser_ReturnsEmptyResponseAndCreatesDatabase()
    {
        UserDataStore store = new(TimeProvider.System);
        GiftServiceImpl service = new(store, TimeProvider.System);

        GetGiftListResponse response = await service.GetGiftList(
            new GetGiftListRequest { IsAscendingSort = false, GetCount = 0 },
            FakeServerCallContext.For(UserId));

        Assert.Empty(response.Gift);
        Assert.Equal(0, response.TotalPageCount);
        Assert.Equal(0, response.NextCursor);
        Assert.Equal(0, response.PreviousCursor);
        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? createdDb));
        Assert.NotNull(createdDb);
        Assert.Empty(createdDb.EntitySUserGift);
    }

    [Fact]
    public async Task GetGiftReceiveHistoryList_ReturnsOnlyReceivedGiftsForCurrentUser()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntitySUserGift.AddRange(
        [
            CreateGift(
                "received-first",
                possessionType: 11,
                possessionId: 101,
                count: 1,
                grantDatetime: 1111,
                expirationDatetime: 2222,
                descriptionGiftTextId: 3333,
                equipmentData: [5],
                receivedDatetime: 4444),
            CreateGift("pending", possessionType: 22, possessionId: 202, count: 2, receivedDatetime: 0),
            CreateGift(
                "received-second",
                possessionType: 33,
                possessionId: 303,
                count: 3,
                grantDatetime: 5555,
                expirationDatetime: 6666,
                descriptionGiftTextId: 7777,
                equipmentData: [7, 8],
                receivedDatetime: 8888),
            CreateGift("other-user", userId: 999L, possessionType: 44, possessionId: 404, count: 4, receivedDatetime: 9999)
        ]);

        GiftServiceImpl service = CreateService(userDb);

        GetGiftReceiveHistoryListResponse response = await service.GetGiftReceiveHistoryList(
            new Empty(),
            FakeServerCallContext.For(UserId));

        Assert.Equal(2, response.Gift.Count);

        ReceivedGift firstGift = response.Gift[0];
        Assert.Equal(11, firstGift.GiftCommon.PossessionType);
        Assert.Equal(101, firstGift.GiftCommon.PossessionId);
        Assert.Equal(1, firstGift.GiftCommon.Count);
        Assert.Equal(3333, firstGift.GiftCommon.DescriptionGiftTextId);
        Assert.Equal(1111, firstGift.GiftCommon.GrantDatetime.ToDateTimeOffset().ToUnixTimeMilliseconds());
        Assert.Equal([5], firstGift.GiftCommon.EquipmentData.ToByteArray());
        Assert.Equal(4444, firstGift.ReceivedDatetime.ToDateTimeOffset().ToUnixTimeMilliseconds());

        ReceivedGift secondGift = response.Gift[1];
        Assert.Equal(33, secondGift.GiftCommon.PossessionType);
        Assert.Equal(303, secondGift.GiftCommon.PossessionId);
        Assert.Equal(3, secondGift.GiftCommon.Count);
        Assert.Equal(7777, secondGift.GiftCommon.DescriptionGiftTextId);
        Assert.Equal(5555, secondGift.GiftCommon.GrantDatetime.ToDateTimeOffset().ToUnixTimeMilliseconds());
        Assert.Equal([7, 8], secondGift.GiftCommon.EquipmentData.ToByteArray());
        Assert.Equal(8888, secondGift.ReceivedDatetime.ToDateTimeOffset().ToUnixTimeMilliseconds());
    }

    private static GiftServiceImpl CreateService(DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return new GiftServiceImpl(store, TimeProvider.System);
    }

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static EntitySUserGift CreateGift(
        string userGiftUuid,
        long userId = UserId,
        int possessionType = 1,
        int possessionId = 2,
        int count = 3,
        long grantDatetime = 0,
        long expirationDatetime = 0,
        int descriptionGiftTextId = 4,
        byte[]? equipmentData = null,
        long receivedDatetime = 0)
        => new()
        {
            UserId = userId,
            UserGiftUuid = userGiftUuid,
            PossessionType = possessionType,
            PossessionId = possessionId,
            Count = count,
            GrantDatetime = grantDatetime,
            ExpirationDatetime = expirationDatetime,
            DescriptionGiftTextId = descriptionGiftTextId,
            EquipmentData = equipmentData ?? [],
            ReceivedDatetime = receivedDatetime
        };
}
