using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;

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
        ServerOptions options = configuration.GetSection(ServerOptions.SectionName).Get<ServerOptions>()!;

        string binPath = Path.Combine(options.Paths.MasterDatabase, $"{options.Data.LatestMasterDataVersion}.bin.e");
        DarkMasterMemoryDatabase masterDb = BinaryMasterDataLoader.Load(binPath);

        GameConfig gameConfig = GameConfig.From(masterDb.EntityMConfig);

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(masterDb);
        services.AddSingleton(gameConfig);
        services.AddSingleton<QuestCampaignHelper>();
        services.AddSingleton<UserDataStore>();
        services.AddSingleton<UserDataSeeder>();

        return services;
    }
}
