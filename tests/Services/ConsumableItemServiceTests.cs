using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.ConsumableItem;
using MariesWonderland.Tests.Infrastructure;
using ConsumableItemServiceImpl = MariesWonderland.Services.ConsumableItemService;

namespace MariesWonderland.Tests.Services;

public class ConsumableItemServiceTests
{
    private const long UserId = 1L;
    private const int GoldItemId = 900;
    private const int PotionItemId = 1001;
    private const int ElixirItemId = 1002;
    private const int EtherItemId = 1003;
    private const int BattlePointPotionItemId = 2001;

    [Fact]
    public async Task UseEffectItem_WithFixedStaminaRecovery_ConsumesItemsAndRestoresStamina()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddConsumableItemEffect(masterDb, PotionItemId, EffectTargetType.STAMINA_RECOVERY, EffectValueType.FIXED_VALUE, effectValue: 3);
        masterDb.EntityMUserLevel.Add(new EntityMUserLevel { UserLevel = 10, MaxStamina = 20 });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = PotionItemId,
            Count = 4,
            FirstAcquisitionDatetime = 100
        });
        userDb.EntityIUserStatus.Add(new EntityIUserStatus
        {
            UserId = UserId,
            Level = 10,
            StaminaMilliValue = 5_000,
            StaminaUpdateDatetime = 50
        });

        ConsumableItemServiceImpl service = CreateService(masterDb, userDb);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        UseEffectItemResponse response = await service.UseEffectItem(
            new UseEffectItemRequest { ConsumableItemId = PotionItemId, Count = 2 },
            FakeServerCallContext.For(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.NotNull(response);
        Assert.Empty(response.DiffUserData);

        EntityIUserConsumableItem item = Assert.Single(userDb.EntityIUserConsumableItem);
        Assert.Equal(PotionItemId, item.ConsumableItemId);
        Assert.Equal(2, item.Count);

        EntityIUserStatus status = Assert.Single(userDb.EntityIUserStatus);
        Assert.Equal(11_000, status.StaminaMilliValue);
        Assert.InRange(status.StaminaUpdateDatetime, beforeMs, afterMs);
    }

    [Fact]
    public async Task UseEffectItem_WithPermilStaminaRecovery_UsesLevelCapForRecovery()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddConsumableItemEffect(masterDb, ElixirItemId, EffectTargetType.STAMINA_RECOVERY, EffectValueType.PERMIL_VALUE, effectValue: 500);
        masterDb.EntityMUserLevel.Add(new EntityMUserLevel { UserLevel = 5, MaxStamina = 20 });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = ElixirItemId,
            Count = 1,
            FirstAcquisitionDatetime = 100
        });
        userDb.EntityIUserStatus.Add(new EntityIUserStatus
        {
            UserId = UserId,
            Level = 5,
            StaminaMilliValue = 15_000,
            StaminaUpdateDatetime = 50
        });

        ConsumableItemServiceImpl service = CreateService(masterDb, userDb);

        await service.UseEffectItem(
            new UseEffectItemRequest { ConsumableItemId = ElixirItemId, Count = 1 },
            FakeServerCallContext.For(UserId));

        Assert.Empty(userDb.EntityIUserConsumableItem);

        EntityIUserStatus status = Assert.Single(userDb.EntityIUserStatus);
        Assert.Equal(20_000, status.StaminaMilliValue);
    }

    [Fact]
    public async Task UseEffectItem_WithUnsupportedEffectTarget_LeavesStateUnchanged()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddConsumableItemEffect(masterDb, BattlePointPotionItemId, EffectTargetType.BATTLE_POINT_RECOVERY, EffectValueType.FIXED_VALUE, effectValue: 10);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = BattlePointPotionItemId,
            Count = 3,
            FirstAcquisitionDatetime = 100
        });
        userDb.EntityIUserStatus.Add(new EntityIUserStatus
        {
            UserId = UserId,
            Level = 1,
            StaminaMilliValue = 5_000,
            StaminaUpdateDatetime = 50
        });

        ConsumableItemServiceImpl service = CreateService(masterDb, userDb);

        await service.UseEffectItem(
            new UseEffectItemRequest { ConsumableItemId = BattlePointPotionItemId, Count = 1 },
            FakeServerCallContext.For(UserId));

        EntityIUserConsumableItem item = Assert.Single(userDb.EntityIUserConsumableItem);
        Assert.Equal(3, item.Count);

        EntityIUserStatus status = Assert.Single(userDb.EntityIUserStatus);
        Assert.Equal(5_000, status.StaminaMilliValue);
        Assert.Equal(50, status.StaminaUpdateDatetime);
    }

    [Fact]
    public async Task Sell_WithMixedEntries_SellsOnlyValidItemsAndUpdatesExistingGold()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddMasterItem(masterDb, PotionItemId, sellPrice: 7);
        AddMasterItem(masterDb, ElixirItemId, sellPrice: 4);
        AddMasterItem(masterDb, EtherItemId, sellPrice: 99);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.AddRange(
        [
            new EntityIUserConsumableItem
            {
                UserId = UserId,
                ConsumableItemId = PotionItemId,
                Count = 3,
                FirstAcquisitionDatetime = 100
            },
            new EntityIUserConsumableItem
            {
                UserId = UserId,
                ConsumableItemId = ElixirItemId,
                Count = 5,
                FirstAcquisitionDatetime = 200
            },
            new EntityIUserConsumableItem
            {
                UserId = UserId,
                ConsumableItemId = GoldItemId,
                Count = 10,
                FirstAcquisitionDatetime = 300
            }
        ]);

        ConsumableItemServiceImpl service = CreateService(masterDb, userDb);

        SellResponse response = await service.Sell(
            new SellRequest
            {
                ConsumableItemPossession =
                {
                    new SellPossession { ConsumableItemId = PotionItemId, Count = 3 },
                    new SellPossession { ConsumableItemId = ElixirItemId, Count = 2 },
                    new SellPossession { ConsumableItemId = EtherItemId, Count = 1 },
                    new SellPossession { ConsumableItemId = 9999, Count = 5 }
                }
            },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.DoesNotContain(userDb.EntityIUserConsumableItem, item => item.ConsumableItemId == PotionItemId);
        EntityIUserConsumableItem elixir = Assert.Single(
            userDb.EntityIUserConsumableItem,
            item => item.ConsumableItemId == ElixirItemId);
        Assert.Equal(3, elixir.Count);

        EntityIUserConsumableItem gold = Assert.Single(
            userDb.EntityIUserConsumableItem,
            item => item.ConsumableItemId == GoldItemId);
        Assert.Equal(39, gold.Count);
        Assert.Equal(300, gold.FirstAcquisitionDatetime);
    }

    [Fact]
    public async Task Sell_WithoutExistingGold_CreatesGoldEntryForSoldTotal()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddMasterItem(masterDb, PotionItemId, sellPrice: 12);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = PotionItemId,
            Count = 2,
            FirstAcquisitionDatetime = 111
        });

        ConsumableItemServiceImpl service = CreateService(masterDb, userDb);

        await service.Sell(
            new SellRequest
            {
                ConsumableItemPossession =
                {
                    new SellPossession { ConsumableItemId = PotionItemId, Count = 2 }
                }
            },
            FakeServerCallContext.For(UserId));

        EntityIUserConsumableItem gold = Assert.Single(userDb.EntityIUserConsumableItem);
        Assert.Equal(UserId, gold.UserId);
        Assert.Equal(GoldItemId, gold.ConsumableItemId);
        Assert.Equal(24, gold.Count);
        Assert.True(gold.FirstAcquisitionDatetime > 0);
    }

    [Fact]
    public async Task Sell_WhenNoEntriesCanBeSold_LeavesInventoryUnchangedAndDoesNotCreateGold()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddMasterItem(masterDb, PotionItemId, sellPrice: 8);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = PotionItemId,
            Count = 1,
            FirstAcquisitionDatetime = 222
        });

        ConsumableItemServiceImpl service = CreateService(masterDb, userDb);

        await service.Sell(
            new SellRequest
            {
                ConsumableItemPossession =
                {
                    new SellPossession { ConsumableItemId = PotionItemId, Count = 2 },
                    new SellPossession { ConsumableItemId = 9999, Count = 1 }
                }
            },
            FakeServerCallContext.For(UserId));

        EntityIUserConsumableItem remainingItem = Assert.Single(userDb.EntityIUserConsumableItem);
        Assert.Equal(PotionItemId, remainingItem.ConsumableItemId);
        Assert.Equal(1, remainingItem.Count);
    }

    private static ConsumableItemServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return CreateService(masterDb, store);
    }

    private static ConsumableItemServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, UserDataStore store)
        => new(masterDb, store, new GameConfig { ConsumableItemIdForGold = GoldItemId, StaminaMaxCount = 999 }, TimeProvider.System);

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static void AddMasterItem(DarkMasterMemoryDatabase masterDb, int consumableItemId, int sellPrice)
    {
        masterDb.EntityMConsumableItem.Add(new EntityMConsumableItem
        {
            ConsumableItemId = consumableItemId,
            SellPrice = sellPrice,
            AssetName = string.Empty
        });
    }

    private static void AddConsumableItemEffect(
        DarkMasterMemoryDatabase masterDb,
        int consumableItemId,
        EffectTargetType targetType,
        EffectValueType valueType,
        int effectValue)
    {
        masterDb.EntityMConsumableItemEffect.Add(new EntityMConsumableItemEffect
        {
            ConsumableItemId = consumableItemId,
            EffectTargetType = targetType,
            EffectValueType = valueType,
            EffectValue = effectValue
        });
    }
}
