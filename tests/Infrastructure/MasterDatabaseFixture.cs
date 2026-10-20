using MariesWonderland.Data;

namespace MariesWonderland.Tests.Infrastructure;

/// <summary>
/// Shared fixture that loads the master database and game config once per test collection.
/// Use as <c>IClassFixture&lt;MasterDatabaseFixture&gt;</c> on test classes that need master data,
/// and mark their tests with <see cref="MasterDataFactAttribute"/> so they skip when it's missing.
/// </summary>
public sealed class MasterDatabaseFixture : IDisposable
{
    public DarkMasterMemoryDatabase MasterDb { get; }
    public GameConfig GameConfig { get; }

    public MasterDatabaseFixture()
    {
        if (!TestSettings.IsMasterDataAvailable)
        {
            // Every test using this fixture is skipped; keep construction from failing.
            MasterDb = new DarkMasterMemoryDatabase();
            GameConfig = GameConfig.From([]);
            return;
        }

        MasterDb = BinaryMasterDataLoader.Load(TestSettings.MasterDataPath);
        GameConfig = GameConfig.From(MasterDb.EntityMConfig);
    }

    public void Dispose() { }
}
