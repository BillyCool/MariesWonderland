using MariesWonderland.Configuration;
using Microsoft.Extensions.Configuration;

namespace MariesWonderland.Tests.Infrastructure;

/// <summary>
/// Resolves server settings for tests the same way the server does: appsettings.json,
/// appsettings.Development.json, the git-ignored appsettings.Local.json, then environment
/// variables (e.g. <c>Server__Paths__MasterDatabase</c>).
/// </summary>
public static class TestSettings
{
    private static readonly Lazy<string> MasterDataPathLazy = new(ResolveMasterDataPath);

    /// <summary>Full path of the master data <c>.bin.e</c> file the tests load.</summary>
    public static string MasterDataPath => MasterDataPathLazy.Value;

    /// <summary>Whether the master data file exists on this machine.</summary>
    public static bool IsMasterDataAvailable => File.Exists(MasterDataPath);

    private static string ResolveMasterDataPath()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        ServerOptions options = config.GetSection(ServerOptions.SectionName).Get<ServerOptions>()!;
        return Path.Combine(options.Paths.MasterDatabase, $"{options.Data.LatestMasterDataVersion}.bin.e");
    }
}
