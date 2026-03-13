using MariesWonderland.Configuration;

namespace MariesWonderland.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServerOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServerOptions>(configuration.GetSection(ServerOptions.SectionName));
        return services;
    }
}
