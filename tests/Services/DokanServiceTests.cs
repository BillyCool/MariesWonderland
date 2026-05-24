using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Dokan;
using MariesWonderland.Tests.Infrastructure;
using DokanServiceImpl = MariesWonderland.Services.DokanService;

namespace MariesWonderland.Tests.Services;

public class DokanServiceTests
{
    private const long UserId = 1L;

    [Fact]
    public async Task RegisterDokanConfirmed_NewIds_CreatesRecordsForEachRequestedChapter()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        UserDataStore store = CreateStore(userDb);
        DokanServiceImpl service = new(store);

        RegisterDokanConfirmedResponse response = await service.RegisterDokanConfirmed(
            new RegisterDokanConfirmedRequest { DokanId = { 101, 202 } },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Collection(
            userDb.EntityIUserDokan.OrderBy(dokan => dokan.DokanId),
            dokan =>
            {
                Assert.Equal(UserId, dokan.UserId);
                Assert.Equal(101, dokan.DokanId);
                Assert.True(dokan.DisplayDatetime > 0);
            },
            dokan =>
            {
                Assert.Equal(UserId, dokan.UserId);
                Assert.Equal(202, dokan.DokanId);
                Assert.True(dokan.DisplayDatetime > 0);
            });

        long timestamp = userDb.EntityIUserDokan[0].DisplayDatetime;
        Assert.All(userDb.EntityIUserDokan, dokan => Assert.Equal(timestamp, dokan.DisplayDatetime));
    }

    [Fact]
    public async Task RegisterDokanConfirmed_ExistingAndDuplicateIds_UpdatesInPlaceWithoutAddingDuplicates()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        EntityIUserDokan existing = new()
        {
            UserId = UserId,
            DokanId = 101,
            DisplayDatetime = 123
        };
        userDb.EntityIUserDokan.Add(existing);

        UserDataStore store = CreateStore(userDb);
        DokanServiceImpl service = new(store);

        await service.RegisterDokanConfirmed(
            new RegisterDokanConfirmedRequest { DokanId = { 101, 303, 303 } },
            FakeServerCallContext.For(UserId));

        Assert.Equal(2, userDb.EntityIUserDokan.Count);
        Assert.Same(existing, Assert.Single(userDb.EntityIUserDokan, dokan => dokan.DokanId == 101));
        Assert.True(existing.DisplayDatetime > 123);

        EntityIUserDokan created = Assert.Single(userDb.EntityIUserDokan, dokan => dokan.DokanId == 303);
        Assert.Equal(UserId, created.UserId);
        Assert.Equal(existing.DisplayDatetime, created.DisplayDatetime);
    }

    [Fact]
    public async Task RegisterDokanConfirmed_WhenUserDatabaseDoesNotExist_CreatesItViaStore()
    {
        UserDataStore store = new(new DarkMasterMemoryDatabase());
        DokanServiceImpl service = new(store);

        await service.RegisterDokanConfirmed(
            new RegisterDokanConfirmedRequest { DokanId = { 404 } },
            FakeServerCallContext.For(UserId));

        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? userDb));

        EntityIUserDokan dokan = Assert.Single(userDb.EntityIUserDokan);
        Assert.Equal(UserId, dokan.UserId);
        Assert.Equal(404, dokan.DokanId);
        Assert.True(dokan.DisplayDatetime > 0);
    }

    [Fact]
    public async Task RegisterDokanConfirmed_EmptyRequest_LeavesExistingRecordsUntouched()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        EntityIUserDokan existing = new()
        {
            UserId = UserId,
            DokanId = 505,
            DisplayDatetime = 999
        };
        userDb.EntityIUserDokan.Add(existing);

        UserDataStore store = CreateStore(userDb);
        DokanServiceImpl service = new(store);

        await service.RegisterDokanConfirmed(
            new RegisterDokanConfirmedRequest(),
            FakeServerCallContext.For(UserId));

        EntityIUserDokan dokan = Assert.Single(userDb.EntityIUserDokan);
        Assert.Same(existing, dokan);
        Assert.Equal(999, dokan.DisplayDatetime);
    }

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static UserDataStore CreateStore(DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(new DarkMasterMemoryDatabase());
        store.Set(UserId, userDb);
        return store;
    }
}
