using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Shop;
using MariesWonderland.Tests.Infrastructure;
using ShopServiceImpl = MariesWonderland.Services.ShopService;

namespace MariesWonderland.Tests.Services;

public class ShopServiceTests
{
    private const long UserId = 1L;
    private const int GemShopItemId = 101;
    private const int PaidGemShopItemId = 102;
    private const int LimitedStockShopItemId = 103;
    private const int DuplicateCostumeShopItemId = 104;
    private const int PoolShopItemA = 201;
    private const int PoolShopItemB = 202;
    private const int RewardConsumableItemId = 901;
    private const int ExchangeConsumableItemId = 902;
    private const int ExistingCostumeId = 701;

    [Fact]
    public async Task Buy_WithAffordableGemItem_DeductsCurrencyGrantsRewardsAndTracksPurchase()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddShopItem(masterDb, GemShopItemId, PriceType.GEM, price: 120);
        masterDb.EntityMShopItemContentPossession.Add(new EntityMShopItemContentPossession
        {
            ShopItemId = GemShopItemId,
            PossessionType = PossessionType.CONSUMABLE_ITEM,
            PossessionId = RewardConsumableItemId,
            Count = 2
        });
        masterDb.EntityMShopItemContentEffect.Add(new EntityMShopItemContentEffect
        {
            ShopItemId = GemShopItemId,
            EffectTargetType = EffectTargetType.STAMINA_RECOVERY,
            EffectValueType = EffectValueType.FIXED_VALUE,
            EffectValue = 7000
        });
        masterDb.EntityMUserLevel.Add(new EntityMUserLevel { UserLevel = 10, MaxStamina = 20 });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = UserId, FreeGem = 100, PaidGem = 50 });
        userDb.EntityIUserStatus.Add(new EntityIUserStatus
        {
            UserId = UserId,
            Level = 10,
            StaminaMilliValue = 14000,
            StaminaUpdateDatetime = 10
        });

        ShopServiceImpl service = CreateService(masterDb, userDb);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        BuyRequest request = new();
        request.ShopItems.Add(GemShopItemId, 1);
        request.ShopItems.Add(9999, 3);

        BuyResponse response = await service.Buy(request, FakeServerCallContext.For(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.NotNull(response);
        Assert.Empty(response.OverflowPossession);
        Assert.Empty(response.DiffUserData);

        EntityIUserGem gem = Assert.Single(userDb.EntityIUserGem);
        Assert.Equal(0, gem.FreeGem);
        Assert.Equal(30, gem.PaidGem);

        EntityIUserConsumableItem reward = Assert.Single(userDb.EntityIUserConsumableItem);
        Assert.Equal(RewardConsumableItemId, reward.ConsumableItemId);
        Assert.Equal(2, reward.Count);

        EntityIUserStatus status = Assert.Single(userDb.EntityIUserStatus);
        Assert.Equal(20000, status.StaminaMilliValue);
        Assert.InRange(status.StaminaUpdateDatetime, beforeMs, afterMs);

        EntityIUserShopItem purchased = Assert.Single(userDb.EntityIUserShopItem);
        Assert.Equal(GemShopItemId, purchased.ShopItemId);
        Assert.Equal(1, purchased.BoughtCount);
        Assert.InRange(purchased.LatestBoughtCountChangedDatetime, beforeMs, afterMs);
    }

    [Fact]
    public async Task Buy_WhenFundsAreInsufficient_SkipsPurchaseAndLeavesStateUnchanged()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddShopItem(masterDb, PaidGemShopItemId, PriceType.PAID_GEM, price: 30);
        masterDb.EntityMShopItemContentPossession.Add(new EntityMShopItemContentPossession
        {
            ShopItemId = PaidGemShopItemId,
            PossessionType = PossessionType.CONSUMABLE_ITEM,
            PossessionId = RewardConsumableItemId,
            Count = 1
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = UserId, FreeGem = 100, PaidGem = 20 });

        ShopServiceImpl service = CreateService(masterDb, userDb);

        BuyRequest request = new();
        request.ShopItems.Add(PaidGemShopItemId, 1);

        await service.Buy(request, FakeServerCallContext.For(UserId));

        EntityIUserGem gem = Assert.Single(userDb.EntityIUserGem);
        Assert.Equal(100, gem.FreeGem);
        Assert.Equal(20, gem.PaidGem);
        Assert.Empty(userDb.EntityIUserConsumableItem);
        Assert.Empty(userDb.EntityIUserShopItem);
    }

    [Fact]
    public async Task Buy_WhenUserAlreadyOwnsCostume_GrantsDuplicationExchangeInsteadOfDuplicateCostume()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddShopItem(masterDb, DuplicateCostumeShopItemId, PriceType.UNKNOWN, price: 0);
        masterDb.EntityMShopItemContentPossession.Add(new EntityMShopItemContentPossession
        {
            ShopItemId = DuplicateCostumeShopItemId,
            PossessionType = PossessionType.COSTUME,
            PossessionId = ExistingCostumeId,
            Count = 1
        });
        masterDb.EntityMCostumeDuplicationExchangePossessionGroup.Add(new EntityMCostumeDuplicationExchangePossessionGroup
        {
            CostumeId = ExistingCostumeId,
            PossessionType = PossessionType.CONSUMABLE_ITEM,
            PossessionId = ExchangeConsumableItemId,
            Count = 3
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCostume.Add(new EntityIUserCostume
        {
            UserId = UserId,
            UserCostumeUuid = "owned-costume",
            CostumeId = ExistingCostumeId,
            AcquisitionDatetime = 1
        });

        ShopServiceImpl service = CreateService(masterDb, userDb);

        BuyRequest request = new();
        request.ShopItems.Add(DuplicateCostumeShopItemId, 2);

        await service.Buy(request, FakeServerCallContext.For(UserId));

        EntityIUserCostume ownedCostume = Assert.Single(userDb.EntityIUserCostume);
        Assert.Equal(ExistingCostumeId, ownedCostume.CostumeId);

        EntityIUserConsumableItem exchange = Assert.Single(userDb.EntityIUserConsumableItem);
        Assert.Equal(ExchangeConsumableItemId, exchange.ConsumableItemId);
        Assert.Equal(6, exchange.Count);

        EntityIUserShopItem purchaseHistory = Assert.Single(userDb.EntityIUserShopItem);
        Assert.Equal(DuplicateCostumeShopItemId, purchaseHistory.ShopItemId);
        Assert.Equal(2, purchaseHistory.BoughtCount);
    }

    [Fact]
    public async Task RefreshUserData_WithoutExistingLineup_SeedsSortedPoolSlots()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddItemShopPool(masterDb, [PoolShopItemA, PoolShopItemB], sortOrderByItemId: new Dictionary<int, int>
        {
            [PoolShopItemA] = 2,
            [PoolShopItemB] = 1
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        ShopServiceImpl service = CreateService(masterDb, userDb);

        RefreshResponse response = await service.RefreshUserData(
            new RefreshRequest { IsGemUsed = false },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Empty(response.DiffUserData);

        Assert.Collection(
            userDb.EntityIUserShopReplaceableLineup.OrderBy(lineup => lineup.SlotNumber),
            first =>
            {
                Assert.Equal(1, first.SlotNumber);
                Assert.Equal(PoolShopItemB, first.ShopItemId);
            },
            second =>
            {
                Assert.Equal(2, second.SlotNumber);
                Assert.Equal(PoolShopItemA, second.ShopItemId);
            });
        Assert.Empty(userDb.EntityIUserShopReplaceable);
    }

    [Fact]
    public async Task RefreshUserData_WhenGemIsUsed_UpdatesReplaceableAndResetsOnlyPoolPurchaseCounts()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddItemShopPool(masterDb, [PoolShopItemA, PoolShopItemB], sortOrderByItemId: new Dictionary<int, int>
        {
            [PoolShopItemA] = 1,
            [PoolShopItemB] = 2
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserShopReplaceableLineup.AddRange(
        [
            new EntityIUserShopReplaceableLineup { UserId = UserId, SlotNumber = 1, ShopItemId = PoolShopItemA },
            new EntityIUserShopReplaceableLineup { UserId = UserId, SlotNumber = 2, ShopItemId = PoolShopItemB }
        ]);
        userDb.EntityIUserShopItem.AddRange(
        [
            new EntityIUserShopItem { UserId = UserId, ShopItemId = PoolShopItemA, BoughtCount = 5 },
            new EntityIUserShopItem { UserId = UserId, ShopItemId = PoolShopItemB, BoughtCount = 2 },
            new EntityIUserShopItem { UserId = UserId, ShopItemId = 999, BoughtCount = 7 }
        ]);

        ShopServiceImpl service = CreateService(masterDb, userDb);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        await service.RefreshUserData(
            new RefreshRequest { IsGemUsed = true },
            FakeServerCallContext.For(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityIUserShopReplaceable replaceable = Assert.Single(userDb.EntityIUserShopReplaceable);
        Assert.Equal(1, replaceable.LineupUpdateCount);
        Assert.InRange(replaceable.LatestLineupUpdateDatetime, beforeMs, afterMs);

        Assert.Equal(0, Assert.Single(userDb.EntityIUserShopItem, item => item.ShopItemId == PoolShopItemA).BoughtCount);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserShopItem, item => item.ShopItemId == PoolShopItemB).BoughtCount);
        Assert.Equal(7, Assert.Single(userDb.EntityIUserShopItem, item => item.ShopItemId == 999).BoughtCount);
        Assert.Equal(2, userDb.EntityIUserShopReplaceableLineup.Count);
    }

    [Fact]
    public async Task CreatePurchaseTransaction_AppliesRewardsUpdatesStockAndReturnsTransactionId()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddShopItem(masterDb, LimitedStockShopItemId, PriceType.PAID_GEM, price: 25, limitedStockId: 1);
        masterDb.EntityMShopItemLimitedStock.Add(new EntityMShopItemLimitedStock
        {
            ShopItemLimitedStockId = 1,
            MaxCount = 2
        });
        masterDb.EntityMShopItemContentPossession.Add(new EntityMShopItemContentPossession
        {
            ShopItemId = LimitedStockShopItemId,
            PossessionType = PossessionType.CONSUMABLE_ITEM,
            PossessionId = RewardConsumableItemId,
            Count = 4
        });
        masterDb.EntityMShopItemContentEffect.Add(new EntityMShopItemContentEffect
        {
            ShopItemId = LimitedStockShopItemId,
            EffectTargetType = EffectTargetType.STAMINA_RECOVERY,
            EffectValueType = EffectValueType.PERMIL_VALUE,
            EffectValue = 500
        });
        masterDb.EntityMUserLevel.Add(new EntityMUserLevel { UserLevel = 5, MaxStamina = 30 });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = UserId, PaidGem = 100, FreeGem = 0 });
        userDb.EntityIUserStatus.Add(new EntityIUserStatus
        {
            UserId = UserId,
            Level = 5,
            StaminaMilliValue = 1000,
            StaminaUpdateDatetime = 5
        });
        userDb.EntityIUserShopItem.Add(new EntityIUserShopItem
        {
            UserId = UserId,
            ShopItemId = LimitedStockShopItemId,
            BoughtCount = 1,
            LatestBoughtCountChangedDatetime = 9
        });

        ShopServiceImpl service = CreateService(masterDb, userDb);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        CreatePurchaseTransactionResponse response = await service.CreatePurchaseTransaction(
            new CreatePurchaseTransactionRequest { ShopItemId = LimitedStockShopItemId },
            FakeServerCallContext.For(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.StartsWith($"tx_{UserId}_{LimitedStockShopItemId}_", response.PurchaseTransactionId);
        Assert.Empty(response.DiffUserData);

        EntityIUserGem gem = Assert.Single(userDb.EntityIUserGem);
        Assert.Equal(75, gem.PaidGem);

        EntityIUserConsumableItem reward = Assert.Single(userDb.EntityIUserConsumableItem);
        Assert.Equal(RewardConsumableItemId, reward.ConsumableItemId);
        Assert.Equal(4, reward.Count);

        EntityIUserStatus status = Assert.Single(userDb.EntityIUserStatus);
        Assert.Equal(16000, status.StaminaMilliValue);
        Assert.InRange(status.StaminaUpdateDatetime, beforeMs, afterMs);

        EntityIUserShopItem purchaseHistory = Assert.Single(userDb.EntityIUserShopItem);
        Assert.Equal(0, purchaseHistory.BoughtCount);
        Assert.InRange(purchaseHistory.LatestBoughtCountChangedDatetime, beforeMs, afterMs);
    }

    [Fact]
    public async Task GetCesaLimitAndPurchaseGooglePlayStoreProduct_ReturnEmptyResponses()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = CreateUserDb();
        ShopServiceImpl service = CreateService(masterDb, userDb);

        GetCesaLimitResponse cesaLimit = await service.GetCesaLimit(new Empty(), FakeServerCallContext.For(UserId));
        PurchaseGooglePlayStoreProductResponse googlePurchase = await service.PurchaseGooglePlayStoreProduct(
            new PurchaseGooglePlayStoreProductRequest { PurchaseTransactionId = "tx" },
            FakeServerCallContext.For(UserId));

        Assert.Empty(cesaLimit.CesaLimit);
        Assert.Empty(cesaLimit.DiffUserData);
        Assert.Empty(googlePurchase.OverflowPossession);
        Assert.Empty(googlePurchase.DiffUserData);
    }

    private static ShopServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return new ShopServiceImpl(store, masterDb, TimeProvider.System);
    }

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static void AddShopItem(
        DarkMasterMemoryDatabase masterDb,
        int shopItemId,
        PriceType priceType,
        int price,
        int limitedStockId = 0)
    {
        masterDb.EntityMShopItem.Add(new EntityMShopItem
        {
            ShopItemId = shopItemId,
            PriceType = priceType,
            Price = price,
            ShopItemLimitedStockId = limitedStockId
        });
    }

    private static void AddItemShopPool(
        DarkMasterMemoryDatabase masterDb,
        IReadOnlyList<int> shopItemIds,
        IReadOnlyDictionary<int, int> sortOrderByItemId)
    {
        const int ShopItemCellGroupId = 301;

        masterDb.EntityMShop.Add(new EntityMShop
        {
            ShopId = 1,
            ShopGroupType = ShopGroupType.ITEM_SHOP,
            ShopItemCellGroupId = ShopItemCellGroupId
        });

        for (int index = 0; index < shopItemIds.Count; index++)
        {
            int shopItemId = shopItemIds[index];
            int cellId = 500 + index;

            masterDb.EntityMShopItemCell.Add(new EntityMShopItemCell
            {
                ShopItemCellId = cellId,
                ShopItemId = shopItemId
            });
            masterDb.EntityMShopItemCellGroup.Add(new EntityMShopItemCellGroup
            {
                ShopItemCellGroupId = ShopItemCellGroupId,
                ShopItemCellId = cellId,
                SortOrder = sortOrderByItemId[shopItemId]
            });
        }
    }
}
