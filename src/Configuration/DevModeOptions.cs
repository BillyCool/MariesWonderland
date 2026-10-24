namespace MariesWonderland.Configuration;

/// <summary>
/// Local development switches. <see cref="Enabled"/> turns every feature on; each feature can
/// also be set individually, which takes precedence over <see cref="Enabled"/>.
/// All features are off by default and should stay off on a hosted server.
/// </summary>
public sealed class DevModeOptions
{
    public const string SectionName = "DevMode";

    /// <summary>Default for every feature below that isn't set explicitly.</summary>
    public bool Enabled { get; init; }

    /// <summary>Write every gRPC request/response pair to files in the GRPC/ directory.</summary>
    public bool? GrpcDiskLog { get; init; }

    /// <summary>Append what every gRPC call changed in the caller's data to a daily file in the Journal/ directory.</summary>
    public bool? RequestJournal { get; init; }

    /// <summary>Map the /debug/save, /debug/load and /debug/saves endpoints (per-user save points).</summary>
    public bool? DebugEndpoints { get; init; }

    /// <summary>
    /// Reload the master data file when it changes, and stamp its write time onto the advertised
    /// master data version so the client downloads the new file.
    /// </summary>
    public bool? MasterDataHotReload { get; init; }

    /// <summary>Starting or skipping a quest costs no stamina.</summary>
    public bool? FreeStamina { get; init; }

    /// <summary>Every summon price is 0 gems or tickets, so banners can be drawn without limit.</summary>
    public bool? FreeGacha { get; init; }

    public bool IsGrpcDiskLogEnabled => GrpcDiskLog ?? Enabled;
    public bool IsRequestJournalEnabled => RequestJournal ?? Enabled;
    public bool IsDebugEndpointsEnabled => DebugEndpoints ?? Enabled;
    public bool IsMasterDataHotReloadEnabled => MasterDataHotReload ?? Enabled;
    public bool IsFreeStaminaEnabled => FreeStamina ?? Enabled;
    public bool IsFreeGachaEnabled => FreeGacha ?? Enabled;
}
