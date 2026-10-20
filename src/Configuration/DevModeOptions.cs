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

    /// <summary>Write the caller's full user database to the Saves/ directory after every gRPC call.</summary>
    public bool? RequestJournal { get; init; }

    /// <summary>Map the /debug/save and /debug/load endpoints.</summary>
    public bool? DebugEndpoints { get; init; }

    public bool IsGrpcDiskLogEnabled => GrpcDiskLog ?? Enabled;
    public bool IsRequestJournalEnabled => RequestJournal ?? Enabled;
    public bool IsDebugEndpointsEnabled => DebugEndpoints ?? Enabled;
}
