using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Companion;
using MariesWonderland.Tests.Infrastructure;
using CompanionServiceImpl = MariesWonderland.Services.CompanionService;

namespace MariesWonderland.Tests.Services;

public class CompanionServiceTests
{
    private const long UserId = 1L;
    private const int GoldItemId = 900;
    private const int CompanionId = 1001;
    private const int CompanionCategoryType = 77;
    private const int GoldCostFunctionId = 2001;
    private const int MaterialAId = 501;
    private const int MaterialBId = 502;

    [Fact]
    public async Task Enhance_WithoutMatchingCompanion_ReturnsEmptyResponseAndLeavesUserDataUntouched()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCompanion(masterDb);
        ConfigureCategory(masterDb, GoldCostFunctionId);
        AddFunction(masterDb, GoldCostFunctionId, NumericalFunctionType.LINEAR, 1, 0);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        AddUserCompanion(userDb, "other-companion", level: 10);
        AddGold(userDb, 100);
        AddMaterial(userDb, MaterialAId, 5);

        CompanionServiceImpl service = CreateService(masterDb, userDb);

        EnhanceResponse response = await service.Enhance(
            new EnhanceRequest { UserCompanionUuid = "missing-companion", AddLevelCount = 3 },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Equal(10, Assert.Single(userDb.EntityIUserCompanion).Level);
        Assert.Equal(100, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(5, Assert.Single(userDb.EntityIUserMaterial).Count);
    }

    [Fact]
    public async Task Enhance_WithoutMasterDefinition_ReturnsEmptyResponseAndLeavesResourcesUntouched()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();

        DarkUserMemoryDatabase userDb = CreateUserDb();
        AddUserCompanion(userDb, "companion-1", level: 5);
        AddGold(userDb, 80);
        AddMaterial(userDb, MaterialAId, 4);

        CompanionServiceImpl service = CreateService(masterDb, userDb);

        EnhanceResponse response = await service.Enhance(
            new EnhanceRequest { UserCompanionUuid = "companion-1", AddLevelCount = 2 },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Equal(5, Assert.Single(userDb.EntityIUserCompanion).Level);
        Assert.Equal(80, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(4, Assert.Single(userDb.EntityIUserMaterial).Count);
    }

    [Fact]
    public async Task Enhance_AppliesPerLevelCostsAndCapsAtMaxLevel()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCompanion(masterDb);
        ConfigureCategory(masterDb, GoldCostFunctionId);
        AddFunction(masterDb, GoldCostFunctionId, NumericalFunctionType.LINEAR, 1, 0);
        AddEnhancementMaterial(masterDb, level: 48, materialId: MaterialAId, count: 2);
        AddEnhancementMaterial(masterDb, level: 49, materialId: MaterialBId, count: 1);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        EntityIUserCompanion companion = AddUserCompanion(userDb, "companion-1", level: 48);
        AddGold(userDb, 200);
        AddMaterial(userDb, MaterialAId, 5);
        AddMaterial(userDb, MaterialBId, 4);

        CompanionServiceImpl service = CreateService(masterDb, userDb);

        await service.Enhance(
            new EnhanceRequest { UserCompanionUuid = "companion-1", AddLevelCount = 5 },
            FakeServerCallContext.For(UserId));

        Assert.Equal(50, companion.Level);
        Assert.Equal(103, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(3, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == MaterialAId).Count);
        Assert.Equal(3, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == MaterialBId).Count);
    }

    [Fact]
    public async Task Enhance_WhenGoldOrMaterialsAreMissing_StillRaisesLevelWithoutCreatingResources()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCompanion(masterDb);
        ConfigureCategory(masterDb, GoldCostFunctionId);
        AddFunction(masterDb, GoldCostFunctionId, NumericalFunctionType.LINEAR, 10, 0);
        AddEnhancementMaterial(masterDb, level: 1, materialId: MaterialAId, count: 2);
        AddEnhancementMaterial(masterDb, level: 2, materialId: MaterialAId, count: 3);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        EntityIUserCompanion companion = AddUserCompanion(userDb, "companion-1", level: 1);
        AddMaterial(userDb, MaterialBId, 7);

        CompanionServiceImpl service = CreateService(masterDb, userDb);

        await service.Enhance(
            new EnhanceRequest { UserCompanionUuid = "companion-1", AddLevelCount = 2 },
            FakeServerCallContext.For(UserId));

        Assert.Equal(3, companion.Level);
        Assert.Empty(userDb.EntityIUserConsumableItem);
        Assert.Equal(7, Assert.Single(userDb.EntityIUserMaterial).Count);
        Assert.Equal(MaterialBId, Assert.Single(userDb.EntityIUserMaterial).MaterialId);
    }

    [Fact]
    public async Task Enhance_WithInsufficientResources_AllowsCountsToGoNegative()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCompanion(masterDb);
        ConfigureCategory(masterDb, GoldCostFunctionId);
        AddFunction(masterDb, GoldCostFunctionId, NumericalFunctionType.LINEAR, 0, 5);
        AddEnhancementMaterial(masterDb, level: 1, materialId: MaterialAId, count: 3);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        EntityIUserCompanion companion = AddUserCompanion(userDb, "companion-1", level: 1);
        AddGold(userDb, 2);
        AddMaterial(userDb, MaterialAId, 1);

        CompanionServiceImpl service = CreateService(masterDb, userDb);

        await service.Enhance(
            new EnhanceRequest { UserCompanionUuid = "companion-1", AddLevelCount = 1 },
            FakeServerCallContext.For(UserId));

        Assert.Equal(2, companion.Level);
        Assert.Equal(-3, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(-2, Assert.Single(userDb.EntityIUserMaterial).Count);
    }

    private static CompanionServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        userDb.UserId = UserId;
        UserDataStore store = new(masterDb, TimeProvider.System);
        store.Set(UserId, userDb);
        return new CompanionServiceImpl(masterDb, store, new GameConfig { ConsumableItemIdForGold = GoldItemId });
    }

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static EntityIUserCompanion AddUserCompanion(DarkUserMemoryDatabase userDb, string uuid, int level)
    {
        EntityIUserCompanion companion = new()
        {
            UserId = UserId,
            UserCompanionUuid = uuid,
            CompanionId = CompanionId,
            Level = level
        };
        userDb.EntityIUserCompanion.Add(companion);
        return companion;
    }

    private static void ConfigureCompanion(DarkMasterMemoryDatabase masterDb)
    {
        masterDb.EntityMCompanion.Add(new EntityMCompanion
        {
            CompanionId = CompanionId,
            CompanionCategoryType = CompanionCategoryType
        });
    }

    private static void ConfigureCategory(DarkMasterMemoryDatabase masterDb, int functionId)
    {
        masterDb.EntityMCompanionCategory.Add(new EntityMCompanionCategory
        {
            CompanionCategoryType = CompanionCategoryType,
            EnhancementCostNumericalFunctionId = functionId
        });
    }

    private static void AddFunction(
        DarkMasterMemoryDatabase masterDb,
        int functionId,
        NumericalFunctionType functionType,
        params int[] parameters)
    {
        masterDb.EntityMNumericalFunction.Add(new EntityMNumericalFunction
        {
            NumericalFunctionId = functionId,
            NumericalFunctionType = functionType,
            NumericalFunctionParameterGroupId = functionId
        });

        for (int index = 0; index < parameters.Length; index++)
        {
            masterDb.EntityMNumericalFunctionParameterGroup.Add(new EntityMNumericalFunctionParameterGroup
            {
                NumericalFunctionParameterGroupId = functionId,
                ParameterIndex = index,
                ParameterValue = parameters[index]
            });
        }
    }

    private static void AddEnhancementMaterial(DarkMasterMemoryDatabase masterDb, int level, int materialId, int count)
    {
        masterDb.EntityMCompanionEnhancementMaterial.Add(new EntityMCompanionEnhancementMaterial
        {
            CompanionCategoryType = CompanionCategoryType,
            Level = level,
            MaterialId = materialId,
            Count = count
        });
    }

    private static void AddGold(DarkUserMemoryDatabase userDb, int count)
    {
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = count
        });
    }

    private static void AddMaterial(DarkUserMemoryDatabase userDb, int materialId, int count)
    {
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = materialId,
            Count = count
        });
    }
}
