using MariesWonderland.Configuration;
using MariesWonderland.Data;
using Microsoft.Extensions.Options;

namespace MariesWonderland.Persistence;

/// <summary>
/// Drops users from memory once they've gone unused for <see cref="DataOptions.IdleUserMinutes"/>. Every change is
/// already saved, so nothing is written here; the next request loads them again.
/// </summary>
public sealed class IdleUserEvictor(UserDataStore store, IOptions<ServerOptions> options, ILogger<IdleUserEvictor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int idleMinutes = options.Value.Data.IdleUserMinutes;
        if (idleMinutes <= 0 || !store.IsPersistent)
        {
            return;
        }

        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            int evicted = store.EvictIdle(TimeSpan.FromMinutes(idleMinutes));
            if (evicted > 0)
            {
                logger.LogInformation("Dropped {Count} idle users from memory", evicted);
            }
        }
    }
}
