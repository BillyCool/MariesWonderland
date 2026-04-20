using MariesWonderland.Configuration;
using MariesWonderland.Data;

namespace MariesWonderland.Extensions;

/// <summary>
/// Dependency injection setup for server configuration and data stores.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Binds the <see cref="ServerOptions"/> configuration section to DI.
    /// </summary>
    public static IServiceCollection AddServerOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServerOptions>(configuration.GetSection(ServerOptions.SectionName));
        return services;
    }

    /// <summary>
    /// Loads the master database and registers <see cref="UserDataStore"/> and related singletons.
    /// </summary>
    public static IServiceCollection AddDataStores(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(ServerOptions.SectionName).Get<ServerOptions>()!;

        var binPath = Path.Combine(options.Paths.MasterDatabase, $"{options.Data.LatestMasterDataVersion}.bin.e");
        var masterDb = BinaryMasterDataLoader.Load(binPath);

        var gameConfig = GameConfig.From(masterDb.EntityMConfig);

        services.AddSingleton(masterDb);
        services.AddSingleton(gameConfig);
        services.AddSingleton<UserDataStore>();
        services.AddSingleton<UserDataSeeder>();

        return services;
    }
}
