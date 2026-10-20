using MariesWonderland.Data;

namespace MariesWonderland.Tests.Infrastructure;

/// <summary>
/// Base class for service-level tests that use the real master data. Concrete test classes take the
/// assembly-wide <see cref="MasterDatabaseFixture"/> in their constructor and pass it here; every
/// test is skipped when the master data file isn't available.
/// </summary>
public abstract class ServiceTestBase
{
    protected DarkMasterMemoryDatabase MasterDb { get; }
    protected GameConfig GameConfig { get; }

    protected ServiceTestBase(MasterDatabaseFixture fixture)
    {
        MasterDatabaseFixture.SkipIfUnavailable();
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
        UserDataStore store = new(masterDb, TimeProvider.System);
        store.Set(userId, userDb);
        return store;
    }

    /// <summary>Shorthand for <see cref="FakeServerCallContext.For"/>.</summary>
    protected static FakeServerCallContext ContextFor(long userId = 1)
        => FakeServerCallContext.For(userId);
}
