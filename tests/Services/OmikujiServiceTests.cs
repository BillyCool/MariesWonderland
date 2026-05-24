using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Omikuji;
using MariesWonderland.Tests.Infrastructure;
using OmikujiServiceImpl = MariesWonderland.Services.OmikujiService;

namespace MariesWonderland.Tests.Services;

public class OmikujiServiceTests
{
    private const long UserId = 1L;
    private const int FirstOmikujiId = 101;
    private const int SecondOmikujiId = 202;
    private const int FirstAssetId = 1001;
    private const int SecondAssetId = 2002;

    private static OmikujiServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, UserDataStore store) =>
        new(store, masterDb);

    private static DarkUserMemoryDatabase CreateUserDb() => new();

    private static UserDataStore CreateStore(long userId, DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb)
    {
        var store = new UserDataStore(masterDb);
        store.Set(userId, userDb);
        return store;
    }

    [Fact]
    public async Task OmikujiDraw_NewOmikuji_AddsRecordWithTimestampAndReturnsAssetId()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMOmikuji.Add(new EntityMOmikuji
        {
            OmikujiId = FirstOmikujiId,
            OmikujiAssetId = FirstAssetId
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        UserDataStore store = CreateStore(UserId, userDb, masterDb);
        OmikujiServiceImpl service = CreateService(masterDb, store);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        OmikujiDrawResponse response = await service.OmikujiDraw(
            new OmikujiDrawRequest { OmikujiId = FirstOmikujiId },
            FakeServerCallContext.For(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.Equal(FirstAssetId, response.OmikujiResultAssetId);

        EntityIUserOmikuji record = Assert.Single(userDb.EntityIUserOmikuji);
        Assert.Equal(UserId, record.UserId);
        Assert.Equal(FirstOmikujiId, record.OmikujiId);
        Assert.InRange(record.LatestDrawDatetime, beforeMs, afterMs);
    }

    [Fact]
    public async Task OmikujiDraw_ExistingOmikuji_UpdatesTimestampWithoutDuplicatingRecord()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMOmikuji.Add(new EntityMOmikuji
        {
            OmikujiId = FirstOmikujiId,
            OmikujiAssetId = FirstAssetId
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserOmikuji.AddRange(
        [
            new EntityIUserOmikuji
            {
                UserId = UserId,
                OmikujiId = FirstOmikujiId,
                LatestDrawDatetime = 123
            },
            new EntityIUserOmikuji
            {
                UserId = UserId,
                OmikujiId = SecondOmikujiId,
                LatestDrawDatetime = 456
            }
        ]);

        UserDataStore store = CreateStore(UserId, userDb, masterDb);
        OmikujiServiceImpl service = CreateService(masterDb, store);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        OmikujiDrawResponse response = await service.OmikujiDraw(
            new OmikujiDrawRequest { OmikujiId = FirstOmikujiId },
            FakeServerCallContext.For(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.Equal(FirstAssetId, response.OmikujiResultAssetId);
        Assert.Equal(2, userDb.EntityIUserOmikuji.Count);

        EntityIUserOmikuji updatedRecord = Assert.Single(
            userDb.EntityIUserOmikuji,
            omikuji => omikuji.OmikujiId == FirstOmikujiId);
        Assert.InRange(updatedRecord.LatestDrawDatetime, beforeMs, afterMs);

        EntityIUserOmikuji untouchedRecord = Assert.Single(
            userDb.EntityIUserOmikuji,
            omikuji => omikuji.OmikujiId == SecondOmikujiId);
        Assert.Equal(456, untouchedRecord.LatestDrawDatetime);
    }

    [Fact]
    public async Task OmikujiDraw_MissingMasterEntry_CreatesUserDatabasePersistsDrawAndReturnsZero()
    {
        DarkMasterMemoryDatabase masterDb = new();
        UserDataStore store = new(masterDb);
        OmikujiServiceImpl service = CreateService(masterDb, store);

        OmikujiDrawResponse response = await service.OmikujiDraw(
            new OmikujiDrawRequest { OmikujiId = FirstOmikujiId },
            FakeServerCallContext.For(UserId));

        Assert.Equal(0, response.OmikujiResultAssetId);
        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? userDb));
        Assert.NotNull(userDb);

        EntityIUserOmikuji record = Assert.Single(userDb.EntityIUserOmikuji);
        Assert.Equal(UserId, record.UserId);
        Assert.Equal(FirstOmikujiId, record.OmikujiId);
        Assert.True(record.LatestDrawDatetime > 0);
    }
}
