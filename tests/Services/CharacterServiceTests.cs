using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Character;
using MariesWonderland.Tests.Infrastructure;
using CharacterServiceImpl = MariesWonderland.Services.CharacterService;

namespace MariesWonderland.Tests.Services;

public class CharacterServiceTests
{
    private const long UserId = 1L;
    private const int CharacterId = 1001;
    private const int GoldItemId = 900;
    private const int MaterialAId = 501;
    private const int MaterialBId = 502;
    private const int RebirthConsumeGold = 100;

    [Fact]
    public async Task Rebirth_WithoutMasterConfiguration_ReturnsEmptyResponseAndLeavesUserDataUntouched()
    {
        DarkMasterMemoryDatabase masterDb = new();
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 250
        });

        CharacterServiceImpl service = CreateService(masterDb, userDb);

        RebirthResponse response = await service.Rebirth(
            new RebirthRequest { CharacterId = CharacterId, RebirthCount = 1 },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Empty(userDb.EntityIUserCharacterRebirth);
        Assert.Equal(250, Assert.Single(userDb.EntityIUserConsumableItem).Count);
    }

    [Fact]
    public async Task Rebirth_WithNewRecord_ConsumesConfiguredResourcesAcrossMultipleSteps()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCharacterRebirth(masterDb, stepGroupId: 10);
        AddStep(masterDb, stepGroupId: 10, beforeRebirthCount: 0, materialGroupId: 100);
        AddStep(masterDb, stepGroupId: 10, beforeRebirthCount: 1, materialGroupId: 101);
        AddMaterial(masterDb, materialGroupId: 100, materialId: MaterialAId, count: 2);
        AddMaterial(masterDb, materialGroupId: 100, materialId: MaterialBId, count: 1);
        AddMaterial(masterDb, materialGroupId: 101, materialId: MaterialAId, count: 3);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 500
        });
        userDb.EntityIUserMaterial.AddRange(
        [
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = MaterialAId,
                Count = 10
            },
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = MaterialBId,
                Count = 1
            }
        ]);

        CharacterServiceImpl service = CreateService(masterDb, userDb);

        RebirthResponse response = await service.Rebirth(
            new RebirthRequest { CharacterId = CharacterId, RebirthCount = 2 },
            FakeServerCallContext.For(UserId));

        EntityIUserCharacterRebirth rebirth = Assert.Single(userDb.EntityIUserCharacterRebirth);
        Assert.NotNull(response);
        Assert.Equal(UserId, rebirth.UserId);
        Assert.Equal(CharacterId, rebirth.CharacterId);
        Assert.Equal(2, rebirth.RebirthCount);
        Assert.Equal(300, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(5, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == MaterialAId).Count);
        Assert.DoesNotContain(userDb.EntityIUserMaterial, material => material.MaterialId == MaterialBId);
    }

    [Fact]
    public async Task Rebirth_WhenStepSequenceEnds_StopsAtLastConfiguredStep()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCharacterRebirth(masterDb, stepGroupId: 20);
        AddStep(masterDb, stepGroupId: 20, beforeRebirthCount: 1, materialGroupId: 200);
        AddMaterial(masterDb, materialGroupId: 200, materialId: MaterialAId, count: 2);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCharacterRebirth.Add(new EntityIUserCharacterRebirth
        {
            UserId = UserId,
            CharacterId = CharacterId,
            RebirthCount = 1
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 500
        });
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = MaterialAId,
            Count = 4
        });

        CharacterServiceImpl service = CreateService(masterDb, userDb);

        await service.Rebirth(
            new RebirthRequest { CharacterId = CharacterId, RebirthCount = 3 },
            FakeServerCallContext.For(UserId));

        Assert.Equal(2, Assert.Single(userDb.EntityIUserCharacterRebirth).RebirthCount);
        Assert.Equal(400, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(2, Assert.Single(userDb.EntityIUserMaterial).Count);
    }

    [Fact]
    public async Task Rebirth_WithInsufficientResources_FloorsGoldAtZeroAndRemovesSpentMaterials()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCharacterRebirth(masterDb, stepGroupId: 30);
        AddStep(masterDb, stepGroupId: 30, beforeRebirthCount: 0, materialGroupId: 300);
        AddMaterial(masterDb, materialGroupId: 300, materialId: MaterialAId, count: 3);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 20
        });
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = MaterialAId,
            Count = 1
        });

        CharacterServiceImpl service = CreateService(masterDb, userDb);

        await service.Rebirth(
            new RebirthRequest { CharacterId = CharacterId, RebirthCount = 1 },
            FakeServerCallContext.For(UserId));

        Assert.Equal(1, Assert.Single(userDb.EntityIUserCharacterRebirth).RebirthCount);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Empty(userDb.EntityIUserMaterial);
    }

    private static CharacterServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return new CharacterServiceImpl(
            masterDb,
            store,
            new GameConfig
            {
                ConsumableItemIdForGold = GoldItemId,
                CharacterRebirthConsumeGold = RebirthConsumeGold
            });
    }

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static void ConfigureCharacterRebirth(DarkMasterMemoryDatabase masterDb, int stepGroupId)
    {
        masterDb.EntityMCharacterRebirth.Add(new EntityMCharacterRebirth
        {
            CharacterId = CharacterId,
            CharacterRebirthStepGroupId = stepGroupId
        });
    }

    private static void AddStep(DarkMasterMemoryDatabase masterDb, int stepGroupId, int beforeRebirthCount, int materialGroupId)
    {
        masterDb.EntityMCharacterRebirthStepGroup.Add(new EntityMCharacterRebirthStepGroup
        {
            CharacterRebirthStepGroupId = stepGroupId,
            BeforeRebirthCount = beforeRebirthCount,
            CharacterRebirthMaterialGroupId = materialGroupId
        });
    }

    private static void AddMaterial(DarkMasterMemoryDatabase masterDb, int materialGroupId, int materialId, int count)
    {
        masterDb.EntityMCharacterRebirthMaterialGroup.Add(new EntityMCharacterRebirthMaterialGroup
        {
            CharacterRebirthMaterialGroupId = materialGroupId,
            MaterialId = materialId,
            Count = count
        });
    }
}
