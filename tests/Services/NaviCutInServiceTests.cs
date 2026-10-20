using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.NaviCutIn;
using MariesWonderland.Tests.Infrastructure;
using NaviCutInServiceImpl = MariesWonderland.Services.NaviCutInService;

namespace MariesWonderland.Tests.Services;

public class NaviCutInServiceTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture)
{
    private const long UserId = 1L;
    private const int FirstNaviCutInId = 101;
    private const int SecondNaviCutInId = 202;

    private static NaviCutInServiceImpl CreateService(UserDataStore store) => new(store);

    [Fact]
    public async Task RegisterPlayed_NewCutIn_AddsRecordWithCurrentTimestamp()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        UserDataStore store = CreateStore(UserId, userDb, MasterDb);
        NaviCutInServiceImpl service = CreateService(store);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        RegisterPlayedResponse response = await service.RegisterPlayed(
            new RegisterPlayedRequest { NaviCutId = FirstNaviCutInId },
            ContextFor(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.NotNull(response);
        EntityIUserNaviCutIn record = Assert.Single(userDb.EntityIUserNaviCutIn);
        Assert.Equal(UserId, record.UserId);
        Assert.Equal(FirstNaviCutInId, record.NaviCutInId);
        Assert.InRange(record.PlayDatetime, beforeMs, afterMs);
    }

    [Fact]
    public async Task RegisterPlayed_ExistingCutIn_UpdatesTimestampWithoutDuplicatingRecord()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserNaviCutIn.AddRange(
        [
            new EntityIUserNaviCutIn
            {
                UserId = UserId,
                NaviCutInId = FirstNaviCutInId,
                PlayDatetime = 123
            },
            new EntityIUserNaviCutIn
            {
                UserId = UserId,
                NaviCutInId = SecondNaviCutInId,
                PlayDatetime = 456
            }
        ]);
        UserDataStore store = CreateStore(UserId, userDb, MasterDb);
        NaviCutInServiceImpl service = CreateService(store);
        long beforeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        await service.RegisterPlayed(
            new RegisterPlayedRequest { NaviCutId = FirstNaviCutInId },
            ContextFor(UserId));

        long afterMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.Equal(2, userDb.EntityIUserNaviCutIn.Count);

        EntityIUserNaviCutIn updatedRecord = Assert.Single(
            userDb.EntityIUserNaviCutIn,
            record => record.NaviCutInId == FirstNaviCutInId);
        Assert.InRange(updatedRecord.PlayDatetime, beforeMs, afterMs);

        EntityIUserNaviCutIn untouchedRecord = Assert.Single(
            userDb.EntityIUserNaviCutIn,
            record => record.NaviCutInId == SecondNaviCutInId);
        Assert.Equal(456, untouchedRecord.PlayDatetime);
    }

    [Fact]
    public async Task RegisterPlayed_MissingUserDatabase_CreatesUserDatabaseAndPersistsRecord()
    {
        UserDataStore store = new(MasterDb);
        NaviCutInServiceImpl service = CreateService(store);

        await service.RegisterPlayed(
            new RegisterPlayedRequest { NaviCutId = FirstNaviCutInId },
            ContextFor(UserId));

        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? userDb));
        Assert.NotNull(userDb);

        EntityIUserNaviCutIn record = Assert.Single(userDb.EntityIUserNaviCutIn);
        Assert.Equal(UserId, record.UserId);
        Assert.Equal(FirstNaviCutInId, record.NaviCutInId);
        Assert.True(record.PlayDatetime > 0);
    }
}
