using System.Text.Json;
using System.Text.Json.Serialization;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Missions;

/// <summary>A cleared quest as missions see it: where it sits and which characters were in the deck.</summary>
public sealed record MissionQuestEvent(QuestCampaignTarget Target, IReadOnlyCollection<int> DeckCharacterIds);

/// <summary>
/// What a mission's clear condition is restricted to. Only the fields an option group sets are non-null, and an
/// event has to match all of them. For quest conditions, <see cref="CharacterId"/> is a character in the deck.
/// </summary>
public sealed record MissionFilter(
    int? CharacterId = null,
    int? BigHuntBossId = null,
    QuestType? QuestType = null,
    EventQuestType? EventQuestType = null,
    int? MainQuestChapterId = null,
    int? EventQuestChapterId = null,
    DifficultyType? Difficulty = null,
    IReadOnlyList<int>? QuestIds = null)
{
    /// <summary>True when the cleared quest meets every restriction this filter sets.</summary>
    public bool MatchesQuest(MissionQuestEvent quest)
    {
        QuestCampaignTarget target = quest.Target;
        return BigHuntBossId == null
            && (QuestType == null || target.QuestType == QuestType)
            && (EventQuestType == null || (target.QuestType == Models.Type.QuestType.EVENT_QUEST && target.EventQuestType == EventQuestType))
            && (MainQuestChapterId == null || (target.QuestType == Models.Type.QuestType.MAIN_QUEST && target.ChapterId == MainQuestChapterId))
            && (EventQuestChapterId == null || (target.QuestType == Models.Type.QuestType.EVENT_QUEST && target.ChapterId == EventQuestChapterId))
            && (Difficulty == null || target.Difficulty == Difficulty)
            && (QuestIds == null || QuestIds.Contains(target.QuestId))
            && (CharacterId == null || quest.DeckCharacterIds.Contains(CharacterId.Value));
    }
}

/// <summary>
/// The mission clear-condition filters (<c>Data/Custom/MissionFilters.json</c>): the server-side table the client never had,
/// rebuilt by hand and keyed by <c>EntityMMission.MissionClearConditionOptionGroupId</c>.
/// </summary>
public sealed class MissionFilterTable
{
    public static readonly string DefaultPath = Path.Combine(AppContext.BaseDirectory, "Data", "Custom", "MissionFilters.json");

    public static readonly MissionFilterTable Empty = new(new Dictionary<int, MissionFilter>());

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

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
            MissionFilter filter = new(
                entry.CharacterId,
                entry.BigHuntBossId,
                entry.QuestType,
                entry.EventQuestType,
                entry.MainQuestChapterId,
                entry.EventQuestChapterId,
                entry.Difficulty,
                entry.QuestIds);
            if (!byOptionGroupId.TryAdd(entry.OptionGroupId, filter))
            {
                throw new InvalidDataException($"Mission filter option group {entry.OptionGroupId} is listed twice in '{path}'.");
            }
        }

        return new MissionFilterTable(byOptionGroupId);
    }

    private sealed record FilterFile(List<FilterEntry> OptionGroups);

    private sealed record FilterEntry(
        int OptionGroupId,
        string? Note,
        int? CharacterId,
        int? BigHuntBossId,
        QuestType? QuestType,
        EventQuestType? EventQuestType,
        int? MainQuestChapterId,
        int? EventQuestChapterId,
        DifficultyType? Difficulty,
        List<int>? QuestIds);
}
