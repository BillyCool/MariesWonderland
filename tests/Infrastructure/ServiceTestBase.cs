using MariesWonderland.Data;

namespace MariesWonderland.Tests.Infrastructure;

/// <summary>
/// Base class for service-level tests. Concrete test classes should implement
/// <c>IClassFixture&lt;MasterDatabaseFixture&gt;</c> and pass the fixture here.
/// </summary>
public abstract class ServiceTestBase
{
    protected DarkMasterMemoryDatabase MasterDb { get; }
    protected GameConfig GameConfig { get; }

    protected ServiceTestBase(MasterDatabaseFixture fixture)
    {
        MasterDb = fixture.MasterDb;
        GameConfig = fixture.GameConfig;
    }

    /// <summary>Creates a fresh empty user database.</summary>
    protected static DarkUserMemoryDatabase CreateUserDb() => new();

    /// <summary>
    /// Creates a <see cref="UserDataStore"/> pre-loaded with the given user database
    /// so that <c>store.GetOrCreate(userId)</c> returns <paramref name="userDb"/>.
    /// </summary>
    protected static UserDataStore CreateStore(long userId, DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb)
    {
        var store = new UserDataStore(masterDb);
        store.Set(userId, userDb);
        return store;
    }

    /// <summary>Shorthand for <see cref="FakeServerCallContext.For"/>.</summary>
    protected static FakeServerCallContext ContextFor(long userId = 1)
        => FakeServerCallContext.For(userId);
}
