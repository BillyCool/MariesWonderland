using System.Text.Json;
using System.Text.Json.Serialization;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Missions;

/// <summary>
/// A cleared quest as missions see it: where it sits, who and which costumes were in the deck, and how many times a
/// day the quest can be cleared (1 for the "Once per Day" quests, 0 when unlimited).
/// </summary>
public sealed record MissionQuestEvent(
    QuestCampaignTarget Target,
    IReadOnlyCollection<int> DeckCharacterIds,
    IReadOnlyCollection<int>? DeckCostumeIds = null,
    int DailyClearableCount = 0);

/// <summary>
/// What a mission's clear condition is restricted to. Only the fields a filter entry sets are non-null, and an event
/// has to match all of them. An entry with no fields matches every event of its condition type.
/// For quest conditions, <see cref="CharacterId"/> is a character in the deck and <see cref="CostumeIds"/> a costume in the deck.
/// </summary>
public sealed record MissionFilter(
    int? CharacterId = null,
    int? BigHuntBossId = null,
    QuestType? QuestType = null,
    EventQuestType? EventQuestType = null,
    int? MainQuestChapterId = null,
    int? EventQuestChapterId = null,
    DifficultyType? Difficulty = null,
    IReadOnlyList<int>? QuestIds = null,
    IReadOnlyList<EventQuestType>? EventQuestTypes = null,
    IReadOnlyList<int>? EventQuestChapterIds = null,
    int? DailyClearableCount = null,
    IReadOnlyList<int>? CostumeIds = null,
    IReadOnlyList<int>? WeaponIds = null,
    IReadOnlyList<int>? ExploreIds = null,
    IReadOnlyList<string>? MessageWords = null,
    int? CharacterBoardGroupId = null,
    IReadOnlyList<DifficultyType>? Difficulties = null,
    GachaLabelType? GachaLabelType = null,
    IReadOnlyList<int>? GachaIds = null,
    bool? IsDailyFreeDraw = null,
    IReadOnlyList<int>? GachaPriceItemIds = null,
    ShopType? ShopType = null)
{
    /// <summary>True when the cleared quest meets every restriction this filter sets.</summary>
    public bool MatchesQuest(MissionQuestEvent quest)
    {
        QuestCampaignTarget target = quest.Target;
        bool isEvent = target.QuestType == Models.Type.QuestType.EVENT_QUEST;
        return BigHuntBossId == null && WeaponIds == null && ExploreIds == null && MessageWords == null && CharacterBoardGroupId == null
            && GachaLabelType == null && GachaIds == null && IsDailyFreeDraw == null && GachaPriceItemIds == null && ShopType == null
            && (QuestType == null || target.QuestType == QuestType)
            && (EventQuestType == null || (isEvent && target.EventQuestType == EventQuestType))
            && (EventQuestTypes == null || (isEvent && EventQuestTypes.Contains(target.EventQuestType)))
            && (MainQuestChapterId == null || (target.QuestType == Models.Type.QuestType.MAIN_QUEST && target.ChapterId == MainQuestChapterId))
            && (EventQuestChapterId == null || (isEvent && target.ChapterId == EventQuestChapterId))
            && (EventQuestChapterIds == null || (isEvent && EventQuestChapterIds.Contains(target.ChapterId)))
            && (Difficulty == null || target.Difficulty == Difficulty)
            && (Difficulties == null || Difficulties.Contains(target.Difficulty))
            && (QuestIds == null || QuestIds.Contains(target.QuestId))
            && (DailyClearableCount == null || quest.DailyClearableCount == DailyClearableCount)
            && (CharacterId == null || quest.DeckCharacterIds.Contains(CharacterId.Value))
            && (CostumeIds == null || (quest.DeckCostumeIds ?? []).Any(CostumeIds.Contains));
    }

    /// <summary>True for an event on this weapon (enhance skills, ascend, evolve: the weapon before evolving).</summary>
    public bool MatchesWeapon(int weaponId) => WeaponIds == null || WeaponIds.Contains(weaponId);

    /// <summary>True for an event on this costume (ascend, awaken, enhance its active skill).</summary>
    public bool MatchesCostume(int costumeId) => CostumeIds == null || CostumeIds.Contains(costumeId);

    /// <summary>True for a run of this exploration.</summary>
    public bool MatchesExplore(int exploreId) => ExploreIds == null || ExploreIds.Contains(exploreId);

    /// <summary>True for a panel unlocked on a board of this Mythic Slab board group (one character's Stone Tower or Cursed God Monument).</summary>
    public bool MatchesBoardGroup(int characterBoardGroupId) => CharacterBoardGroupId == null || CharacterBoardGroupId == characterBoardGroupId;

    /// <summary>
    /// True for a summon of this kind: a banner draw (<paramref name="labelType"/>, with every id the banner goes by,
    /// e.g. a step-up's group id and its first step's master gacha id) or the free daily pull (no banner).
    /// <paramref name="priceItemId"/> is the consumable item the draw was paid with (0 for gems or free): chapter summons
    /// are told apart by their ticket (main chapter tickets vs the Dark Memories Summon Ticket).
    /// </summary>
    public bool MatchesGacha(Models.Type.GachaLabelType? labelType, IReadOnlyCollection<int> gachaIds, bool isDailyFreeDraw, int priceItemId = 0)
        => (GachaLabelType == null || (!isDailyFreeDraw && labelType == GachaLabelType))
            && (GachaIds == null || gachaIds.Any(GachaIds.Contains))
            && (IsDailyFreeDraw == null || IsDailyFreeDraw == isDailyFreeDraw)
            && (GachaPriceItemIds == null || GachaPriceItemIds.Contains(priceItemId));

    /// <summary>True for a purchase in a shop of this type (e.g. the Item Shop).</summary>
    public bool MatchesShop(Models.Type.ShopType shopType) => ShopType == null || ShopType == shopType;

    /// <summary>
    /// True when the profile comment contains one of the words the mission asks for (any comment when none is set).
    /// The Japanese text reads 「…」のワードを入れる, "put the word in", so the comment only has to include it. Each word
    /// is listed in every language the game shipped (English, Japanese, Korean), since players type it in their own.
    /// </summary>
    public bool MatchesMessage(string message) => MessageWords == null || MessageWords.Any(word => message.Contains(word.Trim(), StringComparison.Ordinal));
}

/// <summary>
/// The mission clear-condition filters (<c>Data/Custom/MissionFilters.json</c>): the server-side table the client never had,
/// rebuilt by hand. Entries are keyed by <c>EntityMMission.MissionClearConditionOptionGroupId</c>, or by
/// <c>MissionId</c> for a mission that shares an option group but counts something else; a mission entry wins.
/// </summary>
public sealed class MissionFilterTable
{
    public static readonly string DefaultPath = Path.Combine(AppContext.BaseDirectory, "Data", "Custom", "MissionFilters.json");

    public static readonly MissionFilterTable Empty = new(new Dictionary<int, MissionFilter>());

    // Unknown fields are an error: a field the server doesn't read would leave a mission matching too much.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IReadOnlyDictionary<int, MissionFilter> _byOptionGroupId;
    private readonly IReadOnlyDictionary<int, MissionFilter> _byMissionId;

    public MissionFilterTable(IReadOnlyDictionary<int, MissionFilter> byOptionGroupId, IReadOnlyDictionary<int, MissionFilter>? byMissionId = null)
    {
        _byOptionGroupId = byOptionGroupId;
        _byMissionId = byMissionId ?? new Dictionary<int, MissionFilter>();
    }

    public int Count => _byOptionGroupId.Count + _byMissionId.Count;

    /// <summary>The filter of an option group; null when the option group isn't mapped.</summary>
    public MissionFilter? Find(int optionGroupId) => _byOptionGroupId.GetValueOrDefault(optionGroupId);

    /// <summary>A mission's filter: its own entry if it has one, else its option group's; null when neither is mapped.</summary>
    public MissionFilter? FindForMission(int missionId, int optionGroupId)
        => _byMissionId.GetValueOrDefault(missionId) ?? (optionGroupId == 0 ? null : Find(optionGroupId));

    /// <summary>True when the mission has its own entry.</summary>
    public bool HasMissionEntry(int missionId) => _byMissionId.ContainsKey(missionId);

    public static MissionFilterTable Load(string path)
    {
        FilterFile? file = JsonSerializer.Deserialize<FilterFile>(File.ReadAllText(path), JsonOptions);
        Dictionary<int, MissionFilter> byOptionGroupId = [];
        Dictionary<int, MissionFilter> byMissionId = [];
        foreach (FilterEntry entry in file?.OptionGroups ?? [])
        {
            if ((entry.OptionGroupId == null) == (entry.MissionId == null))
            {
                throw new InvalidDataException($"A mission filter in '{path}' must set exactly one of optionGroupId and missionId (note: {entry.Note}).");
            }

            MissionFilter filter = new(
                entry.CharacterId,
                entry.BigHuntBossId,
                entry.QuestType,
                entry.EventQuestType,
                entry.MainQuestChapterId,
                entry.EventQuestChapterId,
                entry.Difficulty,
                entry.QuestIds,
                entry.EventQuestTypes,
                entry.EventQuestChapterIds,
                entry.DailyClearableCount,
                entry.CostumeIds,
                entry.WeaponIds,
                entry.ExploreIds,
                entry.MessageWords,
                entry.CharacterBoardGroupId,
                entry.Difficulties,
                entry.GachaLabelType,
                entry.GachaIds,
                entry.IsDailyFreeDraw,
                entry.GachaPriceItemIds,
                entry.ShopType);
            (Dictionary<int, MissionFilter> table, int key, string kind) = entry.MissionId is int missionId
                ? (byMissionId, missionId, "mission")
                : (byOptionGroupId, entry.OptionGroupId!.Value, "option group");
            if (!table.TryAdd(key, filter))
            {
                throw new InvalidDataException($"Mission filter {kind} {key} is listed twice in '{path}'.");
            }
        }

        return new MissionFilterTable(byOptionGroupId, byMissionId);
    }

    private sealed record FilterFile(
        [property: JsonPropertyName("_about")] string? About,
        List<FilterEntry> OptionGroups);

    private sealed record FilterEntry(
        int? OptionGroupId,
        int? MissionId,
        string? Note,
        int? CharacterId,
        int? BigHuntBossId,
        QuestType? QuestType,
        EventQuestType? EventQuestType,
        int? MainQuestChapterId,
        int? EventQuestChapterId,
        DifficultyType? Difficulty,
        List<int>? QuestIds,
        List<EventQuestType>? EventQuestTypes,
        List<int>? EventQuestChapterIds,
        int? DailyClearableCount,
        List<int>? CostumeIds,
        List<int>? WeaponIds,
        List<int>? ExploreIds,
        List<string>? MessageWords,
        int? CharacterBoardGroupId,
        List<DifficultyType>? Difficulties,
        GachaLabelType? GachaLabelType,
        List<int>? GachaIds,
        bool? IsDailyFreeDraw,
        List<int>? GachaPriceItemIds,
        ShopType? ShopType);
}
