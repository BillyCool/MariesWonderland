using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.CageOrnament;
using MariesWonderland.Tests.Infrastructure;
using CageOrnamentService = MariesWonderland.Services.CageOrnamentService;

namespace MariesWonderland.Tests.Services;

public class CageOrnamentServiceTests
{
    private const long UserId = 1L;
    private const int CageOrnamentId = 1001;
    private const int CageOrnamentRewardId = 2001;
    private const int ConsumableItemId = 3001;

    [Fact]
    public async Task ReceiveReward_WithValidReward_CreatesRewardRecordAndGrantsPossession()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureReward(masterDb);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        CageOrnamentService service = CreateService(masterDb, userDb);

        ReceiveRewardResponse response = await service.ReceiveReward(
            new ReceiveRewardRequest { CageOrnamentId = CageOrnamentId },
            FakeServerCallContext.For(UserId));

        EntityIUserCageOrnamentReward rewardRecord = Assert.Single(userDb.EntityIUserCageOrnamentReward);
        Assert.Equal(UserId, rewardRecord.UserId);
        Assert.Equal(CageOrnamentId, rewardRecord.CageOrnamentId);
        Assert.True(rewardRecord.AcquisitionDatetime > 0);

        EntityIUserConsumableItem consumable = Assert.Single(userDb.EntityIUserConsumableItem);
        Assert.Equal(UserId, consumable.UserId);
        Assert.Equal(ConsumableItemId, consumable.ConsumableItemId);
        Assert.Equal(3, consumable.Count);
        Assert.True(consumable.FirstAcquisitionDatetime > 0);

        CageOrnamentReward responseReward = Assert.Single(response.CageOrnamentReward);
        Assert.Equal((int)PossessionType.CONSUMABLE_ITEM, responseReward.PossessionType);
        Assert.Equal(ConsumableItemId, responseReward.PossessionId);
        Assert.Equal(3, responseReward.Count);
    }

    [Fact]
    public async Task ReceiveReward_WithExistingRewardRecord_DoesNotDuplicateRecord()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureReward(masterDb);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCageOrnamentReward.Add(new EntityIUserCageOrnamentReward
        {
            UserId = UserId,
            CageOrnamentId = CageOrnamentId,
            AcquisitionDatetime = 12345
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = ConsumableItemId,
            Count = 7,
            FirstAcquisitionDatetime = 67890
        });

        CageOrnamentService service = CreateService(masterDb, userDb);

        await service.ReceiveReward(
            new ReceiveRewardRequest { CageOrnamentId = CageOrnamentId },
            FakeServerCallContext.For(UserId));

        EntityIUserCageOrnamentReward rewardRecord = Assert.Single(userDb.EntityIUserCageOrnamentReward);
        Assert.Equal(12345, rewardRecord.AcquisitionDatetime);

        EntityIUserConsumableItem consumable = Assert.Single(userDb.EntityIUserConsumableItem);
        Assert.Equal(10, consumable.Count);
        Assert.Equal(67890, consumable.FirstAcquisitionDatetime);
    }

    [Fact]
    public async Task ReceiveReward_UnknownOrnament_ReturnsEmptyWithoutCreatingUserData()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        UserDataStore store = new(TimeProvider.System);
        CageOrnamentService service = new(store, masterDb, TimeProvider.System);

        ReceiveRewardResponse response = await service.ReceiveReward(
            new ReceiveRewardRequest { CageOrnamentId = CageOrnamentId },
            FakeServerCallContext.For(UserId));

        Assert.Empty(response.CageOrnamentReward);
        Assert.False(store.TryGet(UserId, out _));
    }

    [Fact]
    public async Task ReceiveReward_MissingRewardDefinition_ReturnsEmptyWithoutPersisting()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        masterDb.EntityMCageOrnament.Add(new EntityMCageOrnament
        {
            CageOrnamentId = CageOrnamentId,
            CageOrnamentRewardId = CageOrnamentRewardId
        });

        UserDataStore store = new(TimeProvider.System);
        CageOrnamentService service = new(store, masterDb, TimeProvider.System);

        ReceiveRewardResponse response = await service.ReceiveReward(
            new ReceiveRewardRequest { CageOrnamentId = CageOrnamentId },
            FakeServerCallContext.For(UserId));

        Assert.Empty(response.CageOrnamentReward);
        Assert.False(store.TryGet(UserId, out _));
    }

    [Fact]
    public async Task RecordAccess_FirstAccess_CreatesRewardRecord()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = CreateUserDb();
        CageOrnamentService service = CreateService(masterDb, userDb);

        await service.RecordAccess(
            new RecordAccessRequest { CageOrnamentId = CageOrnamentId },
            FakeServerCallContext.For(UserId));

        EntityIUserCageOrnamentReward rewardRecord = Assert.Single(userDb.EntityIUserCageOrnamentReward);
        Assert.Equal(UserId, rewardRecord.UserId);
        Assert.Equal(CageOrnamentId, rewardRecord.CageOrnamentId);
        Assert.True(rewardRecord.AcquisitionDatetime > 0);
    }

    [Fact]
    public async Task RecordAccess_ExistingAccess_DoesNotDuplicateRecord()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCageOrnamentReward.Add(new EntityIUserCageOrnamentReward
        {
            UserId = UserId,
            CageOrnamentId = CageOrnamentId,
            AcquisitionDatetime = 2468
        });

        CageOrnamentService service = CreateService(masterDb, userDb);

        await service.RecordAccess(
            new RecordAccessRequest { CageOrnamentId = CageOrnamentId },
            FakeServerCallContext.For(UserId));

        EntityIUserCageOrnamentReward rewardRecord = Assert.Single(userDb.EntityIUserCageOrnamentReward);
        Assert.Equal(2468, rewardRecord.AcquisitionDatetime);
    }

    private static CageOrnamentService CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return new CageOrnamentService(store, masterDb, TimeProvider.System);
    }

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static void ConfigureReward(DarkMasterMemoryDatabase masterDb)
    {
        masterDb.EntityMCageOrnament.Add(new EntityMCageOrnament
        {
            CageOrnamentId = CageOrnamentId,
            CageOrnamentRewardId = CageOrnamentRewardId
        });
        masterDb.EntityMCageOrnamentReward.Add(new EntityMCageOrnamentReward
        {
            CageOrnamentRewardId = CageOrnamentRewardId,
            PossessionType = PossessionType.CONSUMABLE_ITEM,
            PossessionId = ConsumableItemId,
            Count = 3
        });
    }
}
