namespace MariesWonderland.Configuration;

public sealed class ServerOptions
{
    public const string SectionName = "Server";

    public PathsOptions Paths { get; init; } = new();
    public DataOptions Data { get; init; } = new();
}

public sealed class PathsOptions
{
    public string AssetDatabase { get; init; } = string.Empty;
    public string MasterDatabase { get; init; } = string.Empty;

    /// <summary>
    /// Replacement URL written into list.bin in-place when serving asset lists.
    /// Must be exactly 43 ASCII bytes to preserve protobuf field lengths.
    /// Leave empty to serve list.bin unmodified.
    /// </summary>
    public string ResourcesBaseUrl { get; init; } = string.Empty;
}

public sealed class DataOptions
{
    public string LatestMasterDataVersion { get; init; } = string.Empty;
    public string UserDataPath { get; init; } = string.Empty;
    
    /// <summary>
    /// Path of the JSON snapshot file used by <c>/debug/save</c> and <c>/debug/load</c>. On a first start with an
    /// empty user database, its users are imported. If relative, resolved against the application base directory.
    /// </summary>
    public string UserDatabase { get; init; } = "userdata.json";

    /// <summary>
    /// Path of the SQLite user database (created on first start, migrated on every start).
    /// If relative, resolved against the application base directory.
    /// </summary>
    public string SqliteDatabase { get; init; } = "userdata.db";

    /// <summary>Minutes a user may go unused before being dropped from memory (they stay saved). 0 keeps everyone loaded.</summary>
    public int IdleUserMinutes { get; init; } = 30;

    /// <summary>Resolves a data file path: rooted as is, relative against the application base directory.</summary>
    public static string ResolvePath(string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
}
