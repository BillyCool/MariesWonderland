using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Quest;

namespace MariesWonderland.Helpers;

public sealed class QuestCampaignHelper
{
    private readonly IReadOnlyList<QuestCampaignRow> _questCampaigns;
    private readonly IReadOnlyDictionary<int, int> _mainQuestChapterByQuestId;
    private readonly IReadOnlyDictionary<int, DifficultyType> _mainQuestDifficultyByQuestId;
    private readonly IReadOnlyDictionary<int, EventQuestType> _eventQuestTypeByChapterId;
    private readonly IReadOnlyDictionary<int, DifficultyType> _eventQuestDifficultyByQuestId;

    public QuestCampaignHelper(DarkMasterMemoryDatabase masterDb)
    {
        _questCampaigns = LoadQuestCampaigns(masterDb);
        _mainQuestChapterByQuestId = BuildMainQuestChapterLookup(masterDb);
        _mainQuestDifficultyByQuestId = BuildMainQuestDifficultyLookup(masterDb);
        _eventQuestTypeByChapterId = masterDb.EntityMEventQuestChapter
            .GroupBy(chapter => chapter.EventQuestChapterId)
            .ToDictionary(group => group.Key, group => group.First().EventQuestType);
        _eventQuestDifficultyByQuestId = BuildEventQuestDifficultyLookup(masterDb);
    }

    public QuestCampaignTarget GetMainQuestTarget(int questId)
        => new(
            questId,
            QuestType.MAIN_QUEST,
            _mainQuestChapterByQuestId.GetValueOrDefault(questId),
            Difficulty: _mainQuestDifficultyByQuestId.GetValueOrDefault(questId));

    public QuestCampaignTarget GetEventQuestTarget(int questId, int eventQuestChapterId)
        => new(
            questId,
            QuestType.EVENT_QUEST,
            eventQuestChapterId,
            _eventQuestTypeByChapterId.GetValueOrDefault(eventQuestChapterId, EventQuestType.UNKNOWN),
            _eventQuestDifficultyByQuestId.GetValueOrDefault(questId));

    public QuestCampaignTarget GetExtraQuestTarget(int questId)
        => new(questId, QuestType.EXTRA_QUEST);

    public QuestCampaignTarget GetBigHuntQuestTarget(int questId)
        => new(questId, QuestType.BIG_HUNT_QUEST);

    /// <summary>
    /// Applies active drop campaigns: drop-rate campaigns add together into one bonus, which also takes
    /// <paramref name="extraBonusPermil"/> per reward (important items); bonus items are appended unboosted.
    /// Beginner and comeback campaigns apply according to <paramref name="userStatus"/> (none when null).
    /// </summary>
    public List<QuestReward> ApplyDropCampaignBonuses(
        IEnumerable<QuestReward> baseRewards,
        QuestCampaignTarget target,
        long nowMs,
        Func<QuestReward, int>? extraBonusPermil = null,
        UserCampaignStatus? userStatus = null)
    {
        UserCampaignStatus status = userStatus ?? UserCampaignStatus.None;
        int dropRateBonusPermil = 0;
        List<QuestReward> bonusDrops = [];

        foreach (QuestCampaignRow row in _questCampaigns)
        {
            if (!row.IsActive(nowMs, status) || !row.Matches(target))
            {
                continue;
            }

            switch (row.EffectType)
            {
                case QuestCampaignEffectType.DROP_RATE:
                    dropRateBonusPermil += row.EffectValue;
                    break;
                case QuestCampaignEffectType.DROP_ITEM_ADD:
                    bonusDrops.AddRange(row.BonusItems.Select(CloneReward));
                    break;
            }
        }

        List<QuestReward> rewards = [.. baseRewards.Select(CloneReward)];
        foreach (QuestReward reward in rewards)
        {
            int bonusPermil = dropRateBonusPermil + (extraBonusPermil?.Invoke(reward) ?? 0);
            reward.Count = ApplyBonusPermil(reward.Count, bonusPermil);
        }

        rewards.AddRange(bonusDrops);
        return rewards;
    }

    /// <summary>
    /// The stamina a quest costs with the lowest active stamina campaign applied (rounded down). Beginner and comeback
    /// campaigns apply according to <paramref name="userStatus"/> (none when null).
    /// </summary>
    public int ApplyStaminaCampaign(
        int baseStamina,
        QuestCampaignTarget target,
        long nowMs,
        UserCampaignStatus? userStatus = null)
    {
        UserCampaignStatus status = userStatus ?? UserCampaignStatus.None;
        int minPermil = 1000;

        foreach (QuestCampaignRow row in _questCampaigns)
        {
            if (!row.IsActive(nowMs, status) ||
                row.EffectType != QuestCampaignEffectType.STAMINA_CONSUME_AMOUNT ||
                !row.Matches(target))
            {
                continue;
            }

            minPermil = Math.Min(minPermil, row.EffectValue);
        }

        return (int)(baseStamina * (long)minPermil / 1000);
    }

    private static IReadOnlyList<QuestCampaignRow> LoadQuestCampaigns(DarkMasterMemoryDatabase masterDb)
    {
        Dictionary<int, List<QuestCampaignTargetMatch>> targetsByGroupId = [];
        foreach (EntityMQuestCampaignTargetGroup target in masterDb.EntityMQuestCampaignTargetGroup)
        {
            if (!targetsByGroupId.TryGetValue(target.QuestCampaignTargetGroupId, out List<QuestCampaignTargetMatch>? matches))
            {
                matches = [];
                targetsByGroupId[target.QuestCampaignTargetGroupId] = matches;
            }

            matches.Add(new QuestCampaignTargetMatch(target.QuestCampaignTargetType, target.QuestCampaignTargetValue));
        }

        Dictionary<int, EntityMQuestCampaignEffectGroup> effectByGroupId = [];
        foreach (EntityMQuestCampaignEffectGroup effect in masterDb.EntityMQuestCampaignEffectGroup)
        {
            effectByGroupId[effect.QuestCampaignEffectGroupId] = effect;
        }

        Dictionary<int, List<QuestReward>> bonusItemsByGroupId = [];
        foreach (EntityMQuestCampaignTargetItemGroup itemGroup in masterDb.EntityMQuestCampaignTargetItemGroup)
        {
            if (!bonusItemsByGroupId.TryGetValue(itemGroup.QuestCampaignTargetItemGroupId, out List<QuestReward>? rewards))
            {
                rewards = [];
                bonusItemsByGroupId[itemGroup.QuestCampaignTargetItemGroupId] = rewards;
            }

            rewards.Add(new QuestReward
            {
                PossessionType = (int)itemGroup.PossessionType,
                PossessionId = itemGroup.PossessionId,
                Count = itemGroup.Count,
                RewardEffectId = RewardEffectHelper.EffectId(masterDb, itemGroup.PossessionType, itemGroup.PossessionId)
            });
        }

        List<QuestCampaignRow> campaigns = [];
        foreach (EntityMQuestCampaign campaign in masterDb.EntityMQuestCampaign)
        {
            if (!targetsByGroupId.TryGetValue(campaign.QuestCampaignTargetGroupId, out List<QuestCampaignTargetMatch>? targets) ||
                targets.Count == 0 ||
                !effectByGroupId.TryGetValue(campaign.QuestCampaignEffectGroupId, out EntityMQuestCampaignEffectGroup? effect))
            {
                continue;
            }

            campaigns.Add(new QuestCampaignRow(
                effect.QuestCampaignEffectType,
                effect.QuestCampaignEffectValue,
                bonusItemsByGroupId.GetValueOrDefault(effect.QuestCampaignTargetItemGroupId, []),
                targets,
                campaign.StartDatetime,
                campaign.EndDatetime,
                campaign.TargetUserStatusType));
        }

        return campaigns;
    }

    private static IReadOnlyDictionary<int, DifficultyType> BuildEventQuestDifficultyLookup(DarkMasterMemoryDatabase masterDb)
    {
        Dictionary<int, DifficultyType> difficultyBySequenceId = [];
        foreach (EntityMEventQuestSequenceGroup group in masterDb.EntityMEventQuestSequenceGroup)
        {
            difficultyBySequenceId.TryAdd(group.EventQuestSequenceId, group.DifficultyType);
        }

        Dictionary<int, DifficultyType> difficultyByQuestId = [];
        foreach (EntityMEventQuestSequence sequence in masterDb.EntityMEventQuestSequence)
        {
            if (difficultyBySequenceId.TryGetValue(sequence.EventQuestSequenceId, out DifficultyType difficulty))
            {
                difficultyByQuestId.TryAdd(sequence.QuestId, difficulty);
            }
        }

        return difficultyByQuestId;
    }

    private static IReadOnlyDictionary<int, DifficultyType> BuildMainQuestDifficultyLookup(DarkMasterMemoryDatabase masterDb)
    {
        Dictionary<int, DifficultyType> difficultyBySequenceId = [];
        foreach (EntityMMainQuestSequenceGroup group in masterDb.EntityMMainQuestSequenceGroup)
        {
            difficultyBySequenceId.TryAdd(group.MainQuestSequenceId, group.DifficultyType);
        }

        Dictionary<int, DifficultyType> difficultyByQuestId = [];
        foreach (EntityMMainQuestSequence sequence in masterDb.EntityMMainQuestSequence)
        {
            if (difficultyBySequenceId.TryGetValue(sequence.MainQuestSequenceId, out DifficultyType difficulty))
            {
                difficultyByQuestId.TryAdd(sequence.QuestId, difficulty);
            }
        }

        return difficultyByQuestId;
    }

    private static IReadOnlyDictionary<int, int> BuildMainQuestChapterLookup(DarkMasterMemoryDatabase masterDb)
    {
        Dictionary<int, int> sequenceGroupBySequenceId = masterDb.EntityMMainQuestSequenceGroup
            .ToDictionary(group => group.MainQuestSequenceId, group => group.MainQuestSequenceGroupId);

        Dictionary<int, int> chapterBySequenceGroupId = masterDb.EntityMMainQuestChapter
            .GroupBy(chapter => chapter.MainQuestSequenceGroupId)
            .ToDictionary(group => group.Key, group => group.First().MainQuestChapterId);

        Dictionary<int, int> chapterByQuestId = [];
        foreach (EntityMMainQuestSequence sequence in masterDb.EntityMMainQuestSequence)
        {
            if (!sequenceGroupBySequenceId.TryGetValue(sequence.MainQuestSequenceId, out int sequenceGroupId) ||
                !chapterBySequenceGroupId.TryGetValue(sequenceGroupId, out int chapterId))
            {
                continue;
            }

            chapterByQuestId[sequence.QuestId] = chapterId;
        }

        return chapterByQuestId;
    }

    private static QuestReward CloneReward(QuestReward reward)
        => new()
        {
            PossessionType = reward.PossessionType,
            PossessionId = reward.PossessionId,
            Count = reward.Count,
            RewardEffectId = reward.RewardEffectId
        };

    /// <summary>Raises a count by a permil bonus, rounding up.</summary>
    public static int ApplyBonusPermil(int baseCount, int bonusPermil)
        => bonusPermil <= 0 ? baseCount : (int)(((long)baseCount * (1000 + bonusPermil) + 999) / 1000);

    private sealed record QuestCampaignRow(
        QuestCampaignEffectType EffectType,
        int EffectValue,
        IReadOnlyList<QuestReward> BonusItems,
        IReadOnlyList<QuestCampaignTargetMatch> Targets,
        long StartDatetime,
        long EndDatetime,
        TargetUserStatusType TargetUserStatusType)
    {
        public bool IsActive(long nowMs, UserCampaignStatus userStatus)
            => userStatus.Covers(TargetUserStatusType, StartDatetime, EndDatetime, nowMs);

        public bool Matches(QuestCampaignTarget target)
            => Targets.Any(match => match.Matches(target));
    }

    private sealed record QuestCampaignTargetMatch(QuestCampaignTargetType TargetType, int TargetValue)
    {
        public bool Matches(QuestCampaignTarget target)
            => TargetType switch
            {
                QuestCampaignTargetType.WHOLE_QUEST => true,
                QuestCampaignTargetType.QUEST_TYPE => (int)target.QuestType == TargetValue,
                QuestCampaignTargetType.EVENT_QUEST_TYPE
                    => target.QuestType == QuestType.EVENT_QUEST && (int)target.EventQuestType == TargetValue,
                QuestCampaignTargetType.MAIN_QUEST_CHAPTER_ID
                    => target.QuestType == QuestType.MAIN_QUEST && target.ChapterId == TargetValue,
                QuestCampaignTargetType.MAIN_QUEST_QUEST_ID
                    => target.QuestType == QuestType.MAIN_QUEST && target.QuestId == TargetValue,
                QuestCampaignTargetType.SUB_QUEST_CHAPTER_ID
                    => target.QuestType == QuestType.EVENT_QUEST && target.ChapterId == TargetValue,
                QuestCampaignTargetType.SUB_QUEST_QUEST_ID
                    => target.QuestType == QuestType.EVENT_QUEST && target.QuestId == TargetValue,
                _ => false
            };
    }
}

public sealed record QuestCampaignTarget(
    int QuestId,
    QuestType QuestType,
    int ChapterId = 0,
    EventQuestType EventQuestType = EventQuestType.UNKNOWN,
    DifficultyType Difficulty = DifficultyType.UNKNOWN);
