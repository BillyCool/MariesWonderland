using System.Threading.Channels;

namespace MariesWonderland.Data;

/// <summary>
/// Dev-mode file watcher: reloads the master data <c>.bin.e</c> whenever it changes and swaps it into
/// the <see cref="MasterDataHolder"/>. A file that fails to load is logged and the old data stays.
/// </summary>
public sealed class MasterDataReloader(MasterDataHolder holder, string filePath, string baseVersion, ILogger<MasterDataReloader> logger)
    : BackgroundService
{
    // Copying a file raises several change events; wait until it has been quiet this long.
    private static readonly TimeSpan Debounce = TimeSpan.FromMilliseconds(500);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Channel<bool> changes = Channel.CreateBounded<bool>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });

        using FileSystemWatcher watcher = new(Path.GetDirectoryName(Path.GetFullPath(filePath))!, Path.GetFileName(filePath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
        };
        watcher.Changed += (_, _) => changes.Writer.TryWrite(true);
        watcher.Created += (_, _) => changes.Writer.TryWrite(true);
        watcher.Renamed += (_, _) => changes.Writer.TryWrite(true);
        watcher.EnableRaisingEvents = true;

        logger.LogInformation("Watching {File} for master data changes", filePath);

        try
        {
            while (await changes.Reader.WaitToReadAsync(stoppingToken))
            {
                do
                {
                    changes.Reader.TryRead(out _);
                    await Task.Delay(Debounce, stoppingToken);
                }
                while (changes.Reader.TryRead(out _));

                Reload();
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private void Reload()
    {
        try
        {
            MasterDataSnapshot snapshot = MasterDataSnapshot.Load(filePath, baseVersion, stampFileTime: true);
            holder.Replace(snapshot);
            logger.LogInformation("Master data reloaded; client version is now {Version}", snapshot.Version);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Master data reload failed; keeping version {Version}", holder.Current.Version);
        }
    }
}
