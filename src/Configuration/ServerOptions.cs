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
