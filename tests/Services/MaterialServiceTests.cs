using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Material;
using MariesWonderland.Tests.Infrastructure;
using MaterialServiceImpl = MariesWonderland.Services.MaterialService;

namespace MariesWonderland.Tests.Services;

public class MaterialServiceTests
{
    private const long UserId = 1L;
    private const int GoldItemId = 900;
    private const int IronOreId = 1001;
    private const int SilverOreId = 1002;
    private const int GoldOreId = 1003;

    [Fact]
    public async Task Sell_WithMixedEntries_SellsOnlyValidMaterialsAndUpdatesExistingGold()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddMasterMaterial(masterDb, IronOreId, sellPrice: 7);
        AddMasterMaterial(masterDb, SilverOreId, sellPrice: 4);
        AddMasterMaterial(masterDb, GoldOreId, sellPrice: 99);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserMaterial.AddRange(
        [
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = IronOreId,
                Count = 3,
                FirstAcquisitionDatetime = 100
            },
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = SilverOreId,
                Count = 5,
                FirstAcquisitionDatetime = 200
            }
        ]);
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 10,
            FirstAcquisitionDatetime = 300
        });

        MaterialServiceImpl service = CreateService(masterDb, userDb);

        SellResponse response = await service.Sell(
            new SellRequest
            {
                MaterialPossession =
                {
                    new SellPossession { MaterialId = IronOreId, Count = 3 },
                    new SellPossession { MaterialId = SilverOreId, Count = 2 },
                    new SellPossession { MaterialId = GoldOreId, Count = 1 },
                    new SellPossession { MaterialId = 9999, Count = 5 }
                }
            },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.DoesNotContain(userDb.EntityIUserMaterial, item => item.MaterialId == IronOreId);

        EntityIUserMaterial silver = Assert.Single(
            userDb.EntityIUserMaterial,
            item => item.MaterialId == SilverOreId);
        Assert.Equal(3, silver.Count);

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
        AddMasterMaterial(masterDb, IronOreId, sellPrice: 12);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = IronOreId,
            Count = 2,
            FirstAcquisitionDatetime = 111
        });

        MaterialServiceImpl service = CreateService(masterDb, userDb);

        await service.Sell(
            new SellRequest
            {
                MaterialPossession =
                {
                    new SellPossession { MaterialId = IronOreId, Count = 2 }
                }
            },
            FakeServerCallContext.For(UserId));

        Assert.Empty(userDb.EntityIUserMaterial);

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
        AddMasterMaterial(masterDb, IronOreId, sellPrice: 8);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = IronOreId,
            Count = 1,
            FirstAcquisitionDatetime = 222
        });

        MaterialServiceImpl service = CreateService(masterDb, userDb);

        await service.Sell(
            new SellRequest
            {
                MaterialPossession =
                {
                    new SellPossession { MaterialId = IronOreId, Count = 2 },
                    new SellPossession { MaterialId = 9999, Count = 1 }
                }
            },
            FakeServerCallContext.For(UserId));

        EntityIUserMaterial remainingItem = Assert.Single(userDb.EntityIUserMaterial);
        Assert.Equal(IronOreId, remainingItem.MaterialId);
        Assert.Equal(1, remainingItem.Count);
        Assert.Empty(userDb.EntityIUserConsumableItem);
    }

    [Fact]
    public async Task Sell_WithNonPositiveCounts_IgnoresEntriesWithoutChangingInventoryOrGold()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddMasterMaterial(masterDb, IronOreId, sellPrice: 8);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = IronOreId,
            Count = 4,
            FirstAcquisitionDatetime = 222
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 10,
            FirstAcquisitionDatetime = 333
        });

        MaterialServiceImpl service = CreateService(masterDb, userDb);

        await service.Sell(
            new SellRequest
            {
                MaterialPossession =
                {
                    new SellPossession { MaterialId = IronOreId, Count = 0 },
                    new SellPossession { MaterialId = IronOreId, Count = -2 }
                }
            },
            FakeServerCallContext.For(UserId));

        EntityIUserMaterial material = Assert.Single(userDb.EntityIUserMaterial);
        Assert.Equal(4, material.Count);

        EntityIUserConsumableItem gold = Assert.Single(userDb.EntityIUserConsumableItem);
        Assert.Equal(10, gold.Count);
        Assert.Equal(333, gold.FirstAcquisitionDatetime);
    }

    private static MaterialServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(masterDb);
        store.Set(UserId, userDb);
        return CreateService(masterDb, store);
    }

    private static MaterialServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, UserDataStore store)
        => new(masterDb, store, new GameConfig { ConsumableItemIdForGold = GoldItemId });

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static void AddMasterMaterial(DarkMasterMemoryDatabase masterDb, int materialId, int sellPrice)
    {
        masterDb.EntityMMaterial.Add(new EntityMMaterial
        {
            MaterialId = materialId,
            SellPrice = sellPrice,
            AssetName = string.Empty
        });
    }
}
