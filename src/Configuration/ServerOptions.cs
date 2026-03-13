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
    public string MasterDataPath { get; init; } = string.Empty;
}
