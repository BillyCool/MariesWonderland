using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.LoginBonus;
using MariesWonderland.Tests.Infrastructure;
using LoginBonusServiceImpl = MariesWonderland.Services.LoginBonusService;

namespace MariesWonderland.Tests.Services;

public class LoginBonusServiceTests
{
    private const long UserId = 1L;
    private const long ThirtyDaysInMilliseconds = 30L * 24 * 60 * 60 * 1000;

    [Fact]
    public async Task ReceiveStamp_ForNewUser_CreatesTrackerAndDeliversFirstStampReward()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMLoginBonusStamp.Add(new EntityMLoginBonusStamp
        {
            LoginBonusId = 1,
            LowerPageNumber = 1,
            StampNumber = 1,
            RewardPossessionType = PossessionType.FREE_GEM,
            RewardPossessionId = 100,
            RewardCount = 5
        });

        UserDataStore store = CreateStore(masterDb);
        LoginBonusServiceImpl service = new(store, masterDb, TimeProvider.System);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        ReceiveStampResponse response = await service.ReceiveStamp(new Empty(), FakeServerCallContext.For(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.NotNull(response);
        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? userDb));
        Assert.NotNull(userDb);

        EntityIUserLoginBonus tracker = Assert.Single(userDb.EntityIUserLoginBonus);
        Assert.Equal(UserId, tracker.UserId);
        Assert.Equal(1, tracker.LoginBonusId);
        Assert.Equal(1, tracker.CurrentPageNumber);
        Assert.Equal(1, tracker.CurrentStampNumber);
        Assert.InRange(tracker.LatestRewardReceiveDatetime, beforeMs, afterMs);

        EntitySUserGift gift = Assert.Single(userDb.EntitySUserGift);
        Assert.Equal(UserId, gift.UserId);
        Assert.Equal("login-bonus-1-1-1", gift.UserGiftUuid);
        Assert.Equal((int)PossessionType.FREE_GEM, gift.PossessionType);
        Assert.Equal(100, gift.PossessionId);
        Assert.Equal(5, gift.Count);
        Assert.Equal(0, gift.ReceivedDatetime);
        Assert.InRange(gift.GrantDatetime, beforeMs, afterMs);
        Assert.Equal(ThirtyDaysInMilliseconds, gift.ExpirationDatetime - gift.GrantDatetime);
        Assert.Equal(tracker.LatestRewardReceiveDatetime, gift.GrantDatetime);
    }

    [Fact]
    public async Task ReceiveStamp_WithExistingTracker_UsesMatchingBonusPageAndNextStampWithoutDuplicatingTracker()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMLoginBonusStamp.AddRange(
        [
            new EntityMLoginBonusStamp
            {
                LoginBonusId = 7,
                LowerPageNumber = 2,
                StampNumber = 4,
                RewardPossessionType = PossessionType.MATERIAL,
                RewardPossessionId = 300,
                RewardCount = 7
            },
            new EntityMLoginBonusStamp
            {
                LoginBonusId = 7,
                LowerPageNumber = 1,
                StampNumber = 4,
                RewardPossessionType = PossessionType.FREE_GEM,
                RewardPossessionId = 999,
                RewardCount = 99
            },
            new EntityMLoginBonusStamp
            {
                LoginBonusId = 1,
                LowerPageNumber = 2,
                StampNumber = 4,
                RewardPossessionType = PossessionType.PAID_GEM,
                RewardPossessionId = 888,
                RewardCount = 88
            }
        ]);

        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserLoginBonus.AddRange(
        [
            new EntityIUserLoginBonus
            {
                UserId = UserId,
                LoginBonusId = 7,
                CurrentPageNumber = 2,
                CurrentStampNumber = 3
            },
            new EntityIUserLoginBonus
            {
                UserId = 999L,
                LoginBonusId = 7,
                CurrentPageNumber = 2,
                CurrentStampNumber = 9
            }
        ]);

        UserDataStore store = CreateStore(masterDb, userDb);
        LoginBonusServiceImpl service = new(store, masterDb, TimeProvider.System);

        await service.ReceiveStamp(new Empty(), FakeServerCallContext.For(UserId));

        EntityIUserLoginBonus tracker = Assert.Single(userDb.EntityIUserLoginBonus, lb => lb.UserId == UserId);
        Assert.Equal(7, tracker.LoginBonusId);
        Assert.Equal(2, tracker.CurrentPageNumber);
        Assert.Equal(4, tracker.CurrentStampNumber);
        Assert.True(tracker.LatestRewardReceiveDatetime > 0);

        EntitySUserGift gift = Assert.Single(userDb.EntitySUserGift);
        Assert.Equal("login-bonus-1-2-4", gift.UserGiftUuid);
        Assert.Equal((int)PossessionType.MATERIAL, gift.PossessionType);
        Assert.Equal(300, gift.PossessionId);
        Assert.Equal(7, gift.Count);
    }

    [Fact]
    public async Task ReceiveStamp_WhenCurrentPageIsExhausted_AdvancesToNextPageFirstStamp()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMLoginBonus.Add(new EntityMLoginBonus
        {
            LoginBonusId = 3,
            TotalPageCount = 2
        });
        masterDb.EntityMLoginBonusStamp.AddRange(
        [
            new EntityMLoginBonusStamp
            {
                LoginBonusId = 3,
                LowerPageNumber = 1,
                StampNumber = 1,
                RewardPossessionType = PossessionType.MATERIAL,
                RewardPossessionId = 111,
                RewardCount = 2
            },
            new EntityMLoginBonusStamp
            {
                LoginBonusId = 3,
                LowerPageNumber = 2,
                StampNumber = 1,
                RewardPossessionType = PossessionType.IMPORTANT_ITEM,
                RewardPossessionId = 222,
                RewardCount = 1
            }
        ]);

        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserLoginBonus.Add(new EntityIUserLoginBonus
        {
            UserId = UserId,
            LoginBonusId = 3,
            CurrentPageNumber = 1,
            CurrentStampNumber = 1
        });

        UserDataStore store = CreateStore(masterDb, userDb);
        LoginBonusServiceImpl service = new(store, masterDb, TimeProvider.System);

        await service.ReceiveStamp(new Empty(), FakeServerCallContext.For(UserId));

        EntityIUserLoginBonus tracker = Assert.Single(userDb.EntityIUserLoginBonus);
        Assert.Equal(2, tracker.CurrentPageNumber);
        Assert.Equal(1, tracker.CurrentStampNumber);

        EntitySUserGift gift = Assert.Single(userDb.EntitySUserGift);
        Assert.Equal("login-bonus-1-2-1", gift.UserGiftUuid);
        Assert.Equal((int)PossessionType.IMPORTANT_ITEM, gift.PossessionType);
        Assert.Equal(222, gift.PossessionId);
        Assert.Equal(1, gift.Count);
    }

    [Fact]
    public async Task ReceiveStamp_WhenLoginBonusIsExhausted_ThrowsFailedPreconditionWithoutMutatingState()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMLoginBonus.Add(new EntityMLoginBonus
        {
            LoginBonusId = 4,
            TotalPageCount = 1
        });
        masterDb.EntityMLoginBonusStamp.Add(new EntityMLoginBonusStamp
        {
            LoginBonusId = 4,
            LowerPageNumber = 1,
            StampNumber = 1,
            RewardPossessionType = PossessionType.FREE_GEM,
            RewardPossessionId = 999,
            RewardCount = 10
        });

        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserLoginBonus.Add(new EntityIUserLoginBonus
        {
            UserId = UserId,
            LoginBonusId = 4,
            CurrentPageNumber = 1,
            CurrentStampNumber = 1
        });

        UserDataStore store = CreateStore(masterDb, userDb);
        LoginBonusServiceImpl service = new(store, masterDb, TimeProvider.System);

        RpcException exception = await Assert.ThrowsAsync<RpcException>(
            () => service.ReceiveStamp(new Empty(), FakeServerCallContext.For(UserId)));

        Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);
        EntityIUserLoginBonus tracker = Assert.Single(userDb.EntityIUserLoginBonus);
        Assert.Equal(1, tracker.CurrentPageNumber);
        Assert.Equal(1, tracker.CurrentStampNumber);
        Assert.Equal(0, tracker.LatestRewardReceiveDatetime);
        Assert.Empty(userDb.EntitySUserGift);
    }

    private static UserDataStore CreateStore(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase? userDb = null)
    {
        UserDataStore store = new(TimeProvider.System);
        if (userDb is not null)
            store.Set(UserId, userDb);

        return store;
    }
}
