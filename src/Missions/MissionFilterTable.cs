using System.Text.Json;

namespace MariesWonderland.Missions;

/// <summary>
/// What a mission's clear condition is restricted to, e.g. one character or one Big Hunt boss.
/// Only the fields an option group sets are non-null.
/// </summary>
public sealed record MissionFilter(int? CharacterId = null, int? BigHuntBossId = null);

/// <summary>
/// The mission clear-condition filters (<c>Data/Custom/MissionFilters.json</c>): the server-side table the client never had,
/// rebuilt by hand and keyed by <c>EntityMMission.MissionClearConditionOptionGroupId</c>.
/// </summary>
public sealed class MissionFilterTable
{
    public static readonly string DefaultPath = Path.Combine(AppContext.BaseDirectory, "Data", "Custom", "MissionFilters.json");

    public static readonly MissionFilterTable Empty = new(new Dictionary<int, MissionFilter>());

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IReadOnlyDictionary<int, MissionFilter> _byOptionGroupId;

    public MissionFilterTable(IReadOnlyDictionary<int, MissionFilter> byOptionGroupId)
    {
        _byOptionGroupId = byOptionGroupId;
    }

    public int Count => _byOptionGroupId.Count;

    /// <summary>The filter of an option group; null when the option group isn't mapped.</summary>
    public MissionFilter? Find(int optionGroupId) => _byOptionGroupId.GetValueOrDefault(optionGroupId);

    public static MissionFilterTable Load(string path)
    {
        FilterFile? file = JsonSerializer.Deserialize<FilterFile>(File.ReadAllText(path), JsonOptions);
        Dictionary<int, MissionFilter> byOptionGroupId = [];
        foreach (FilterEntry entry in file?.OptionGroups ?? [])
        {
            if (!byOptionGroupId.TryAdd(entry.OptionGroupId, new MissionFilter(entry.CharacterId, entry.BigHuntBossId)))
            {
                throw new InvalidDataException($"Mission filter option group {entry.OptionGroupId} is listed twice in '{path}'.");
            }
        }

        return new MissionFilterTable(byOptionGroupId);
    }

    private sealed record FilterFile(List<FilterEntry> OptionGroups);

    private sealed record FilterEntry(int OptionGroupId, string? Note, int? CharacterId, int? BigHuntBossId);
}
