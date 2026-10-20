using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Missions;
using MariesWonderland.Quests;

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
    /// Master data and everything derived from it is resolved per request from the
    /// <see cref="MasterDataHolder"/>, so a dev-mode hot reload takes effect on the next request.
    /// </summary>
    public static IServiceCollection AddDataStores(this IServiceCollection services, IConfiguration configuration, DevModeOptions devMode)
    {
        ServerOptions options = configuration.GetSection(ServerOptions.SectionName).Get<ServerOptions>()!;

        string baseVersion = options.Data.LatestMasterDataVersion;
        string binPath = Path.Combine(options.Paths.MasterDatabase, $"{baseVersion}.bin.e");
        bool hotReload = devMode.IsMasterDataHotReloadEnabled;
        MasterDataHolder holder = new(MasterDataSnapshot.Load(binPath, baseVersion, stampFileTime: hotReload));

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(holder);
        services.AddScoped(provider => provider.GetRequiredService<MasterDataHolder>().Current);
        services.AddScoped(provider => provider.GetRequiredService<MasterDataSnapshot>().Database);
        services.AddScoped(provider => provider.GetRequiredService<MasterDataSnapshot>().GameConfig);
        services.AddScoped(provider => provider.GetRequiredService<MasterDataSnapshot>().QuestCampaigns);
        services.AddScoped(provider => provider.GetRequiredService<MasterDataSnapshot>().EnhanceCampaigns);
        services.AddScoped(provider => provider.GetRequiredService<MasterDataSnapshot>().ImportantItems);
        services.AddScoped(provider => provider.GetRequiredService<MasterDataSnapshot>().Missions);
        services.AddScoped<MissionTracker>();
        services.AddScoped<QuestEngine>();
        if (hotReload)
        {
            services.AddHostedService(provider => new MasterDataReloader(
                holder, binPath, baseVersion, provider.GetRequiredService<ILogger<MasterDataReloader>>()));
        }

        services.AddSingleton<UserDataStore>();
        services.AddSingleton<UserDataSeeder>();

        return services;
    }
}
