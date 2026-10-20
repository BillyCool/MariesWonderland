using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Quest;

namespace MariesWonderland.Helpers;

/// <summary>
/// Reward boosts from owned important items (e.g. "Dungeon Drop Rate+", "Daily Reward+"). Each owned item's effect
/// adds a permil bonus to matching rewards; bonuses from several items add together, and add to quest campaigns.
/// </summary>
public sealed class ImportantItemEffectHelper
{
    // ImportantItemEffectType values; the master data has no enum for them.
    private const int DropRateEffectType = 2;
    private const int DropCountEffectType = 3;

    private readonly IReadOnlyList<ItemEffectRow> _effects;

    public ImportantItemEffectHelper(DarkMasterMemoryDatabase masterDb)
    {
        Dictionary<int, List<QuestGroupMatch>> questGroups = masterDb.EntityMImportantItemEffectTargetQuestGroup
            .GroupBy(g => g.ImportantItemEffectTargetQuestGroupId)
            .ToDictionary(g => g.Key, g => g.Select(r => new QuestGroupMatch(r.ImportantItemEffectTargetQuestGroupType, r.TargetValue)).ToList());
        Dictionary<int, HashSet<(PossessionType, int)>> itemGroups = masterDb.EntityMImportantItemEffectTargetItemGroup
            .GroupBy(g => g.ImportantItemEffectTargetItemGroupId)
            .ToDictionary(g => g.Key, g => g.Select(r => (r.PossessionType, r.PossessionId)).ToHashSet());
        Dictionary<int, EntityMImportantItemEffect> effectsById = masterDb.EntityMImportantItemEffect
            .ToDictionary(e => e.ImportantItemEffectId);

        List<ItemEffectRow> effects = [];
        foreach (EntityMImportantItem item in masterDb.EntityMImportantItem)
        {
            if (!effectsById.TryGetValue(item.ImportantItemEffectId, out EntityMImportantItemEffect? effect))
            {
                continue;
            }

            (int permil, int questGroupId, int itemGroupId) = effect.ImportantItemEffectType switch
            {
                DropCountEffectType => masterDb.EntityMImportantItemEffectDropCount
                    .Where(d => d.ImportantItemEffectDropCountId == effect.ImportantItemEffectTargetId)
                    .Select(d => (d.CountPermil, d.ImportantItemEffectTargetQuestGroupId, d.ImportantItemEffectTargetItemGroupId))
                    .FirstOrDefault(),
                DropRateEffectType => masterDb.EntityMImportantItemEffectDropRate
                    .Where(d => d.ImportantItemEffectDropRateId == effect.ImportantItemEffectTargetId)
                    .Select(d => (d.RatePermil, d.ImportantItemEffectTargetQuestGroupId, d.ImportantItemEffectTargetItemGroupId))
                    .FirstOrDefault(),
                _ => default
            };
            if (permil == 0 || !questGroups.TryGetValue(questGroupId, out List<QuestGroupMatch>? quests))
            {
                continue;
            }

            // Item group 0 means every item.
            HashSet<(PossessionType, int)>? items = itemGroupId == 0 ? null : itemGroups.GetValueOrDefault(itemGroupId, []);
            effects.Add(new ItemEffectRow(item.ImportantItemId, permil, effect.StartDatetime, effect.EndDatetime, quests, items));
        }

        _effects = effects;
    }

    /// <summary>
    /// Sum of the bonuses (permil) the user's important items give a reward of this possession in this quest.
    /// Drop-rate effects count as an amount bonus: quest drops are a fixed list with no odds to raise.
    /// </summary>
    public int DropBonusPermil(DarkUserMemoryDatabase userDb, QuestCampaignTarget target, PossessionType possessionType, int possessionId, long nowMs)
    {
        int sum = 0;
        foreach (ItemEffectRow effect in _effects)
        {
            if (nowMs >= effect.StartDatetime && nowMs <= effect.EndDatetime &&
                (effect.Items == null || effect.Items.Contains((possessionType, possessionId))) &&
                effect.Quests.Any(q => q.Matches(target)) &&
                userDb.EntityIUserImportantItem.Any(i => i.ImportantItemId == effect.ImportantItemId && i.Count > 0))
            {
                sum += effect.Permil;
            }
        }

        return sum;
    }

    /// <summary>Bonus for a reward, as <see cref="DropBonusPermil"/>.</summary>
    public int DropBonusPermil(DarkUserMemoryDatabase userDb, QuestCampaignTarget target, QuestReward reward, long nowMs)
        => DropBonusPermil(userDb, target, (PossessionType)reward.PossessionType, reward.PossessionId, nowMs);

    private sealed record ItemEffectRow(
        int ImportantItemId,
        int Permil,
        long StartDatetime,
        long EndDatetime,
        IReadOnlyList<QuestGroupMatch> Quests,
        HashSet<(PossessionType, int)>? Items);

    private sealed record QuestGroupMatch(ImportantItemEffectTargetQuestGroupType TargetType, int TargetValue)
    {
        public bool Matches(QuestCampaignTarget target)
            => TargetType switch
            {
                ImportantItemEffectTargetQuestGroupType.WHOLE_QUEST => true,
                ImportantItemEffectTargetQuestGroupType.QUEST_TYPE => (int)target.QuestType == TargetValue,
                ImportantItemEffectTargetQuestGroupType.EVENT_QUEST_TYPE
                    => target.QuestType == QuestType.EVENT_QUEST && (int)target.EventQuestType == TargetValue,
                ImportantItemEffectTargetQuestGroupType.MAIN_QUEST_CHAPTER_ID
                    => target.QuestType == QuestType.MAIN_QUEST && target.ChapterId == TargetValue,
                ImportantItemEffectTargetQuestGroupType.MAIN_QUEST_QUEST_ID
                    => target.QuestType == QuestType.MAIN_QUEST && target.QuestId == TargetValue,
                ImportantItemEffectTargetQuestGroupType.SUB_QUEST_CHAPTER_ID
                    => target.QuestType == QuestType.EVENT_QUEST && target.ChapterId == TargetValue,
                ImportantItemEffectTargetQuestGroupType.SUB_QUEST_QUEST_ID
                    => target.QuestType == QuestType.EVENT_QUEST && target.QuestId == TargetValue,
                ImportantItemEffectTargetQuestGroupType.MAIN_QUEST_DIFFICULTY_TYPE
                    => target.QuestType == QuestType.MAIN_QUEST && (int)target.Difficulty == TargetValue,
                _ => false
            };
    }
}
