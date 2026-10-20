using MariesWonderland.Data;
using MariesWonderland.Tests.Infrastructure;

[assembly: AssemblyFixture(typeof(MasterDatabaseFixture))]

namespace MariesWonderland.Tests.Infrastructure;

/// <summary>
/// Assembly-wide fixture that loads the master database and game config once per test run.
/// Test classes that need master data take it as a constructor parameter (usually via
/// <see cref="ServiceTestBase"/>) and call <see cref="SkipIfUnavailable"/> before using it.
/// </summary>
public sealed class MasterDatabaseFixture
{
    public DarkMasterMemoryDatabase MasterDb { get; }
    public GameConfig GameConfig { get; }

    public MasterDatabaseFixture()
    {
        if (!TestSettings.IsMasterDataAvailable)
        {
            // Tests that need master data skip themselves; keep construction from failing.
            MasterDb = new DarkMasterMemoryDatabase();
            GameConfig = GameConfig.From([]);
            return;
        }

        MasterDb = BinaryMasterDataLoader.Load(TestSettings.MasterDataPath);
        GameConfig = GameConfig.From(MasterDb.EntityMConfig);
    }

    /// <summary>Skips the current test when the master data file isn't available on this machine.</summary>
    public static void SkipIfUnavailable()
        => Assert.SkipUnless(TestSettings.IsMasterDataAvailable,
            $"Master data not found at '{TestSettings.MasterDataPath}'. " +
            "Set Server:Paths:MasterDatabase in src/appsettings.Local.json or the Server__Paths__MasterDatabase environment variable.");
}
