using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Parts;
using MariesWonderland.Tests.Infrastructure;
using PartsService = MariesWonderland.Services.PartsService;

namespace MariesWonderland.Tests.Services;

public class PartsServiceTests
{
    private const long UserId = 1L;
    private const int GoldItemId = 900;
    private const int PartsId = 1001;

    [Fact]
    public async Task Enhance_SuccessfullyUnlocksNewSubStatusSlots()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigurePartsMaster(masterDb, sellFunctionId: 0);
        masterDb.EntityMPartsEnhanced.Add(new EntityMPartsEnhanced
        {
            PartsEnhancedId = 501,
            PartsId = PartsId,
            Level = 3,
            SubStatusCount = 1
        });
        masterDb.EntityMPartsEnhancedSubStatus.Add(new EntityMPartsEnhancedSubStatus
        {
            PartsEnhancedId = 501,
            StatusIndex = 1,
            PartsStatusSubLotteryId = 77,
            Level = 3,
            StatusKindType = StatusKindType.ATTACK,
            StatusCalculationType = StatusCalculationType.ADD,
            FixedStatusChangeValue = 150
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 100
        });
        userDb.EntityIUserParts.Add(new EntityIUserParts
        {
            UserId = UserId,
            UserPartsUuid = "parts-1",
            PartsId = PartsId,
            Level = 2
        });

        PartsService service = CreateService(masterDb, userDb);

        EnhanceResponse response = await service.Enhance(
            new EnhanceRequest { UserPartsUuid = "parts-1" },
            FakeServerCallContext.For(UserId));

        Assert.True(response.IsSuccess);
        Assert.Equal(3, userDb.EntityIUserParts.Single().Level);
        Assert.Equal(90, userDb.EntityIUserConsumableItem.Single().Count);

        EntityIUserPartsStatusSub subStatus = Assert.Single(userDb.EntityIUserPartsStatusSub);
        Assert.Equal(UserId, subStatus.UserId);
        Assert.Equal("parts-1", subStatus.UserPartsUuid);
        Assert.Equal(1, subStatus.StatusIndex);
        Assert.Equal(77, subStatus.PartsStatusSubLotteryId);
        Assert.Equal(3, subStatus.Level);
        Assert.Equal(StatusKindType.ATTACK, subStatus.StatusKindType);
        Assert.Equal(StatusCalculationType.ADD, subStatus.StatusCalculationType);
        Assert.Equal(150, subStatus.StatusChangeValue);
        Assert.True(subStatus.LatestVersion > 0);
    }

    [Fact]
    public async Task Enhance_DoesNotDuplicateExistingSubStatusSlot()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigurePartsMaster(masterDb, sellFunctionId: 0);
        masterDb.EntityMPartsEnhanced.Add(new EntityMPartsEnhanced
        {
            PartsEnhancedId = 502,
            PartsId = PartsId,
            Level = 3,
            SubStatusCount = 1
        });
        masterDb.EntityMPartsEnhancedSubStatus.Add(new EntityMPartsEnhancedSubStatus
        {
            PartsEnhancedId = 502,
            StatusIndex = 1,
            PartsStatusSubLotteryId = 88,
            Level = 3,
            StatusKindType = StatusKindType.HP,
            StatusCalculationType = StatusCalculationType.MULTIPLY,
            FixedStatusChangeValue = 300
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 100
        });
        userDb.EntityIUserParts.Add(new EntityIUserParts
        {
            UserId = UserId,
            UserPartsUuid = "parts-1",
            PartsId = PartsId,
            Level = 2
        });
        userDb.EntityIUserPartsStatusSub.Add(new EntityIUserPartsStatusSub
        {
            UserId = UserId,
            UserPartsUuid = "parts-1",
            StatusIndex = 1,
            PartsStatusSubLotteryId = 999,
            Level = 2,
            StatusKindType = StatusKindType.AGILITY,
            StatusCalculationType = StatusCalculationType.ADD,
            StatusChangeValue = 42,
            LatestVersion = 123
        });

        PartsService service = CreateService(masterDb, userDb);

        EnhanceResponse response = await service.Enhance(
            new EnhanceRequest { UserPartsUuid = "parts-1" },
            FakeServerCallContext.For(UserId));

        Assert.True(response.IsSuccess);
        EntityIUserPartsStatusSub subStatus = Assert.Single(userDb.EntityIUserPartsStatusSub);
        Assert.Equal(999, subStatus.PartsStatusSubLotteryId);
        Assert.Equal(42, subStatus.StatusChangeValue);
        Assert.Equal(123, subStatus.LatestVersion);
    }

    [Fact]
    public async Task Sell_RemovesAttachedSubStatusesForSoldPartsOnly()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigurePartsMaster(masterDb, sellFunctionId: 11);
        masterDb.EntityMNumericalFunction.Add(new EntityMNumericalFunction
        {
            NumericalFunctionId = 11,
            NumericalFunctionType = NumericalFunctionType.LINEAR,
            NumericalFunctionParameterGroupId = 21
        });
        masterDb.EntityMNumericalFunctionParameterGroup.AddRange(
        [
            new EntityMNumericalFunctionParameterGroup
            {
                NumericalFunctionParameterGroupId = 21,
                ParameterIndex = 0,
                ParameterValue = 0
            },
            new EntityMNumericalFunctionParameterGroup
            {
                NumericalFunctionParameterGroupId = 21,
                ParameterIndex = 1,
                ParameterValue = 100
            }
        ]);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 5
        });
        userDb.EntityIUserParts.AddRange(
        [
            new EntityIUserParts
            {
                UserId = UserId,
                UserPartsUuid = "sold-part",
                PartsId = PartsId,
                Level = 4
            },
            new EntityIUserParts
            {
                UserId = UserId,
                UserPartsUuid = "kept-part",
                PartsId = PartsId,
                Level = 1
            }
        ]);
        userDb.EntityIUserPartsStatusSub.AddRange(
        [
            new EntityIUserPartsStatusSub
            {
                UserId = UserId,
                UserPartsUuid = "sold-part",
                StatusIndex = 1
            },
            new EntityIUserPartsStatusSub
            {
                UserId = UserId,
                UserPartsUuid = "sold-part",
                StatusIndex = 2
            },
            new EntityIUserPartsStatusSub
            {
                UserId = UserId,
                UserPartsUuid = "kept-part",
                StatusIndex = 1
            }
        ]);

        PartsService service = CreateService(masterDb, userDb);

        await service.Sell(
            new SellRequest { UserPartsUuid = { "sold-part" } },
            FakeServerCallContext.For(UserId));

        Assert.Single(userDb.EntityIUserParts);
        Assert.Equal("kept-part", userDb.EntityIUserParts.Single().UserPartsUuid);
        Assert.Single(userDb.EntityIUserPartsStatusSub);
        Assert.Equal("kept-part", userDb.EntityIUserPartsStatusSub.Single().UserPartsUuid);
        Assert.Equal(105, userDb.EntityIUserConsumableItem.Single().Count);
    }

    private static PartsService CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        userDb.UserId = UserId;
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return new PartsService(masterDb, store, new GameConfig { ConsumableItemIdForGold = GoldItemId }, TimeProvider.System);
    }

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static void ConfigurePartsMaster(DarkMasterMemoryDatabase masterDb, int sellFunctionId)
    {
        masterDb.EntityMParts.Add(new EntityMParts
        {
            PartsId = PartsId,
            RarityType = RarityType.RARE
        });
        masterDb.EntityMPartsRarity.Add(new EntityMPartsRarity
        {
            RarityType = RarityType.RARE,
            PartsLevelUpRateGroupId = 1,
            PartsLevelUpPriceGroupId = 1,
            SellPriceNumericalFunctionId = sellFunctionId
        });
        masterDb.EntityMPartsLevelUpRateGroup.Add(new EntityMPartsLevelUpRateGroup
        {
            PartsLevelUpRateGroupId = 1,
            LevelLowerLimit = 2,
            SuccessRatePermil = 1000
        });
        masterDb.EntityMPartsLevelUpPriceGroup.Add(new EntityMPartsLevelUpPriceGroup
        {
            PartsLevelUpPriceGroupId = 1,
            LevelLowerLimit = 2,
            Gold = 10
        });
    }
}
