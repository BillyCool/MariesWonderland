using MariesWonderland.Configuration;
using MariesWonderland.Data;
using Microsoft.Extensions.Configuration;

namespace MariesWonderland.Tests.Infrastructure;

/// <summary>
/// Shared fixture that loads the master database and game config once per test collection.
/// Use as <c>IClassFixture&lt;MasterDatabaseFixture&gt;</c> on test classes that need master data.
/// </summary>
public sealed class MasterDatabaseFixture : IDisposable
{
    public DarkMasterMemoryDatabase MasterDb { get; }
    public GameConfig GameConfig { get; }

    public MasterDatabaseFixture()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var options = config.GetSection(ServerOptions.SectionName).Get<ServerOptions>()!;
        var binPath = Path.Combine(options.Paths.MasterDatabase, $"{options.Data.LatestMasterDataVersion}.bin.e");

        MasterDb = BinaryMasterDataLoader.Load(binPath);
        GameConfig = GameConfig.From(MasterDb.EntityMConfig);
    }

    public void Dispose() { }
}
