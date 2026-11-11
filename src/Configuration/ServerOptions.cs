namespace MariesWonderland.Configuration;

public sealed class ServerOptions
{
    public const string SectionName = "Server";

    public PathsOptions Paths { get; init; } = new();
    public DataOptions Data { get; init; } = new();
    public GrpcOptions Grpc { get; init; } = new();
}

public sealed class GrpcOptions
{
    /// <summary>
    /// Whether gRPC messages are encrypted like the original client does (see <c>Middleware/GrpcPayloadCipher</c>).
    /// Turn off only for a client whose <c>HandleNet.Encrypt</c>/<c>Decrypt</c> are patched to pass messages through.
    /// </summary>
    public bool EncryptMessages { get; init; } = true;
}

public sealed class PathsOptions
{
    public string AssetDatabase { get; init; } = string.Empty;
    public string MasterDatabase { get; init; } = string.Empty;

    /// <summary>
    /// Asset base URL written into list.bin in place of the original CDN. Must be exactly 43 ASCII characters
    /// (list.bin's field length), http or https, with a path that is one run of 'a's (e.g.
    /// <c>http://192.168.1.2/aaaaaaaaaaaaaaaaaaaaaaaa</c>); otherwise it's ignored with a warning at startup. Leave
    /// empty to use the address the client reached the server on, padded to that length (see <c>Http/ResourcesUrl</c>).
    /// </summary>
    public string ResourcesBaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// The game's English <c>all_localizations.txt</c> (UTF-16), for item and event names in the admin tool.
    /// Optional: without it the admin tool shows ids.
    /// </summary>
    public string Localization { get; init; } = string.Empty;
}

/// <summary>The web app: the transfer site and the admin tool (<c>Web/</c>).</summary>
public sealed class WebOptions
{
    public const string SectionName = "Web";

    /// <summary>
    /// When set, <c>/admin</c> (and the admin tool's live connection) only answers on this local port, e.g. a second
    /// Kestrel endpoint that is only published to the LAN. 0 serves it on every endpoint.
    /// </summary>
    public int AdminPort { get; init; }

    /// <summary>
    /// First administrator, created at startup when there is none: <c>Web:InitialAdmin:Name</c> and
    /// <c>Web:InitialAdmin:Password</c>. Without it, the startup log prints a one-time setup link.
    /// </summary>
    public InitialAdminOptions InitialAdmin { get; init; } = new();

    /// <summary>Whether anyone may upload a save on <c>/import</c> (it waits for an administrator's approval).</summary>
    public bool PublicSaveImport { get; init; } = true;

    /// <summary>
    /// Largest zip accepted on <c>/import</c>, in kilobytes. A full endgame save (107 table files, 10.3 MB) zips to
    /// about 530 KB.
    /// </summary>
    public int MaxImportZipKilobytes { get; init; } = 1024;

    /// <summary>Largest size the uploaded zip may unpack to, in megabytes (the same save unpacks to 10.3 MB).</summary>
    public int MaxImportUnzippedMegabytes { get; init; } = 16;
}

public sealed class InitialAdminOptions
{
    public string Name { get; init; } = "admin";
    public string Password { get; init; } = string.Empty;
}

public sealed class DataOptions
{
    public string LatestMasterDataVersion { get; init; } = string.Empty;
    public string UserDataPath { get; init; } = string.Empty;
    
    /// <summary>
    /// Path of the whole-server JSON snapshot the server kept before it had a user database. On a first start with an
    /// empty user database, its users are imported. If relative, resolved against the application base directory.
    /// </summary>
    public string UserDatabase { get; init; } = "userdata.json";

    /// <summary>
    /// Directory of the per-user save points made by <c>/debug/save</c> and restored by <c>/debug/load</c>.
    /// If relative, resolved against the application base directory.
    /// </summary>
    public string SavesDirectory { get; init; } = "Saves";

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
