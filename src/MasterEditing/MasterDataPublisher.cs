using MariesWonderland.Accounts;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Text.Json;

namespace MariesWonderland.MasterEditing;

/// <summary>
/// Writes a <see cref="MasterDraft"/> into the served master data file and swaps it in live. The new data goes out
/// as version <c>&lt;base&gt;_&lt;file write time&gt;</c> (as dev-mode hot reload does), so every client sees a
/// version change, returns to the title and downloads the file again. Each publish is recorded with its row
/// changes, which gives the history page and its undo. When the file sits next to the git-ignored JSON mirror
/// (<c>EntityM*Table.json</c>, the dev setup), the changed tables are written there too.
/// </summary>
public sealed class MasterDataPublisher(
    MasterDataHolder holder,
    MasterDraft draft,
    AccountStore accounts,
    Func<UserDbContext> createContext,
    IOptions<ServerOptions> options,
    ILogger<MasterDataPublisher> logger)
{
    /// <summary>Server setting: once a publish happened, versions carry the file time from the next start on too.</summary>
    public const string StampedSettingKey = "MasterDataVersionStamped";

    private readonly Lock _publishLock = new();

    public string BaseVersion => options.Value.Data.LatestMasterDataVersion;

    public string FilePath => Path.Combine(options.Value.Paths.MasterDatabase, $"{BaseVersion}.bin.e");

    /// <summary>
    /// At startup: if an earlier publish changed the file, advertise the stamped version, so a client that cached
    /// the file before that publish downloads it again.
    /// </summary>
    public void ApplyStartupVersion()
    {
        if (accounts.GetSetting(StampedSettingKey) == "true" && !holder.Current.Version.Contains('_') && File.Exists(FilePath))
        {
            string version = $"{BaseVersion}_{new DateTimeOffset(File.GetLastWriteTimeUtc(FilePath)).ToUnixTimeMilliseconds()}";
            holder.Replace(holder.Current.WithVersion(version));
            logger.LogInformation("Master data was edited in the admin tool; advertising version {Version}", version);
        }
    }

    /// <summary>Publishes the draft. Returns the new version.</summary>
    public string Publish(string admin, string summary)
    {
        lock (_publishLock)
        {
            if (draft.IsStale)
            {
                throw new InvalidOperationException("The live master data changed after this draft was started. Discard the draft and make the edits again.");
            }

            IReadOnlyList<MasterRowChange> changes = draft.Changes();
            IReadOnlyDictionary<MasterTable, IList> tables = draft.Snapshot();
            if (tables.Count == 0)
            {
                throw new InvalidOperationException("Nothing to publish.");
            }

            MasterDataFile file = MasterDataFile.Read(FilePath);
            Dictionary<string, byte[]> replaced = tables.ToDictionary(t => t.Key.FileName, t => MasterDataFile.Serialize(t.Key, t.Value));
            byte[] bytes = file.Write(replaced);

            // Check the new file loads before it replaces the served one.
            string temp = FilePath + ".publishing";
            File.WriteAllBytes(temp, bytes);
            try
            {
                BinaryMasterDataLoader.Load(temp);
            }
            catch
            {
                File.Delete(temp);
                throw;
            }
            File.Move(temp, FilePath, overwrite: true);

            MasterDataSnapshot snapshot = MasterDataSnapshot.Load(FilePath, BaseVersion, stampFileTime: true);
            holder.Replace(snapshot);
            accounts.SetSetting(StampedSettingKey, "true");
            WriteMirror(tables);

            using (UserDbContext db = createContext())
            {
                db.MasterDataEdits.Add(new MasterDataEdit
                {
                    At = DateTime.UtcNow,
                    Admin = admin,
                    Summary = summary,
                    Version = snapshot.Version,
                    ChangesJson = JsonSerializer.Serialize(changes),
                });
                db.SaveChanges();
            }

            accounts.Audit(admin, "master.publish", string.Join(", ", tables.Keys.Select(t => t.FileName)), $"{changes.Count} row changes, version {snapshot.Version}: {summary}");
            logger.LogInformation("Master data published by {Admin}: {Count} row changes in {Tables}; version {Version}",
                admin, changes.Count, string.Join(", ", tables.Keys.Select(t => t.FileName)), snapshot.Version);
            draft.Discard();
            return snapshot.Version;
        }
    }

    public IReadOnlyList<MasterDataEdit> History(int count = 100)
    {
        using UserDbContext db = createContext();
        return [.. db.MasterDataEdits.AsNoTracking().OrderByDescending(e => e.Id).Take(count)];
    }

    public static IReadOnlyList<MasterRowChange> ChangesOf(MasterDataEdit edit)
        => JsonSerializer.Deserialize<List<MasterRowChange>>(edit.ChangesJson) ?? [];

    private void WriteMirror(IReadOnlyDictionary<MasterTable, IList> tables)
    {
        string folder = options.Value.Paths.MasterDatabase;
        JsonSerializerOptions json = new() { WriteIndented = true, NewLine = "\r\n" };
        foreach ((MasterTable table, IList rows) in tables)
        {
            string path = Path.Combine(folder, $"{table.Name}Table.json");
            if (!File.Exists(path))
            {
                continue;
            }

            try
            {
                Array array = Array.CreateInstance(table.RowType, rows.Count);
                rows.CopyTo(array, 0);
                File.WriteAllText(path, JsonSerializer.Serialize(array, array.GetType(), json));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Couldn't update the JSON mirror {Path}", path);
            }
        }
    }
}
