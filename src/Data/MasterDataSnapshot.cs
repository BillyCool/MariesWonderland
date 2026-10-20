using MariesWonderland.Helpers;

namespace MariesWonderland.Data;

/// <summary>
/// One loaded copy of the master data together with everything derived from it. A request resolves
/// a single snapshot, so the database, config and helpers it sees always belong together.
/// </summary>
public sealed class MasterDataSnapshot
{
    public DarkMasterMemoryDatabase Database { get; }
    public GameConfig GameConfig { get; }
    public QuestCampaignHelper QuestCampaigns { get; }
    public EnhanceCampaignHelper EnhanceCampaigns { get; }

    /// <summary>The version string advertised to the client; a change makes it download the file again.</summary>
    public string Version { get; }

    public MasterDataSnapshot(DarkMasterMemoryDatabase database, string version)
    {
        Database = database;
        GameConfig = GameConfig.From(database.EntityMConfig);
        QuestCampaigns = new QuestCampaignHelper(database);
        EnhanceCampaigns = new EnhanceCampaignHelper(database);
        Version = version;
    }

    /// <summary>
    /// Loads the <c>.bin.e</c> file. With <paramref name="stampFileTime"/> the file's last write time is
    /// appended to <paramref name="baseVersion"/>, so every edit of the file gives the client a new version.
    /// </summary>
    public static MasterDataSnapshot Load(string filePath, string baseVersion, bool stampFileTime)
    {
        DateTimeOffset lastWrite = File.GetLastWriteTimeUtc(filePath);
        DarkMasterMemoryDatabase database = BinaryMasterDataLoader.Load(filePath);
        string version = stampFileTime ? $"{baseVersion}_{lastWrite.ToUnixTimeMilliseconds()}" : baseVersion;
        return new MasterDataSnapshot(database, version);
    }

    /// <summary>Strips a file-time stamp from a version the client requests, leaving the file's base version.</summary>
    public static string BaseVersion(string version)
    {
        int separator = version.IndexOf('_');
        return separator < 0 ? version : version[..separator];
    }
}
