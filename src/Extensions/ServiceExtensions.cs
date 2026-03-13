using MariesWonderland.Configuration;
using MariesWonderland.Data;

namespace MariesWonderland.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServerOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServerOptions>(configuration.GetSection(ServerOptions.SectionName));
        return services;
    }

    public static IServiceCollection AddDataStores(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(ServerOptions.SectionName).Get<ServerOptions>()!;
        var masterDataPath = Path.IsPathRooted(options.Data.MasterDataPath)
            ? options.Data.MasterDataPath
            : Path.Combine(AppContext.BaseDirectory, options.Data.MasterDataPath);

        var masterDb = MasterDataLoader.Load(masterDataPath);
        services.AddSingleton(masterDb);
        services.AddSingleton<UserDataStore>();

        return services;
    }
}
