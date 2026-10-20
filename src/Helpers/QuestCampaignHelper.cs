using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Quest;

namespace MariesWonderland.Helpers;

public sealed class QuestCampaignHelper
{
    private readonly IReadOnlyList<QuestCampaignRow> _questCampaigns;
    private readonly IReadOnlyDictionary<int, int> _mainQuestChapterByQuestId;
    private readonly IReadOnlyDictionary<int, EventQuestType> _eventQuestTypeByChapterId;

    public QuestCampaignHelper(DarkMasterMemoryDatabase masterDb)
    {
        _questCampaigns = LoadQuestCampaigns(masterDb);
        _mainQuestChapterByQuestId = BuildMainQuestChapterLookup(masterDb);
        _eventQuestTypeByChapterId = masterDb.EntityMEventQuestChapter
            .GroupBy(chapter => chapter.EventQuestChapterId)
            .ToDictionary(group => group.Key, group => group.First().EventQuestType);
    }

    public QuestCampaignTarget GetMainQuestTarget(int questId)
        => new(questId, QuestType.MAIN_QUEST, _mainQuestChapterByQuestId.GetValueOrDefault(questId));

    public QuestCampaignTarget GetEventQuestTarget(int questId, int eventQuestChapterId)
        => new(
            questId,
            QuestType.EVENT_QUEST,
            eventQuestChapterId,
            _eventQuestTypeByChapterId.GetValueOrDefault(eventQuestChapterId, EventQuestType.UNKNOWN));

    public QuestCampaignTarget GetExtraQuestTarget(int questId)
        => new(questId, QuestType.EXTRA_QUEST);

    public QuestCampaignTarget GetBigHuntQuestTarget(int questId)
        => new(questId, QuestType.BIG_HUNT_QUEST);

    public List<QuestReward> ApplyDropCampaignBonuses(
        IEnumerable<QuestReward> baseRewards,
        QuestCampaignTarget target,
        long nowMs,
        TargetUserStatusType userStatus = TargetUserStatusType.ALL)
    {
        int maxDropRateBonusPermil = 0;
        List<QuestReward> bonusDrops = [];

        foreach (QuestCampaignRow row in _questCampaigns)
        {
            if (!row.IsActive(nowMs, userStatus) || !row.Matches(target))
            {
                continue;
            }

            switch (row.EffectType)
            {
                case QuestCampaignEffectType.DROP_RATE:
                    maxDropRateBonusPermil = Math.Max(maxDropRateBonusPermil, row.EffectValue);
                    break;
                case QuestCampaignEffectType.DROP_ITEM_ADD:
                    bonusDrops.AddRange(row.BonusItems.Select(CloneReward));
                    break;
            }
        }

        List<QuestReward> rewards = [.. baseRewards.Select(CloneReward)];
        if (maxDropRateBonusPermil > 0)
        {
            foreach (QuestReward reward in rewards)
            {
                reward.Count = ApplyDropRateBonus(reward.Count, maxDropRateBonusPermil);
            }
        }

        rewards.AddRange(bonusDrops);
        return rewards;
    }

    public int ApplyStaminaCampaign(
        int baseStamina,
        QuestCampaignTarget target,
        long nowMs,
        TargetUserStatusType userStatus = TargetUserStatusType.ALL)
    {
        int minPermil = 1000;

        foreach (QuestCampaignRow row in _questCampaigns)
        {
            if (!row.IsActive(nowMs, userStatus) ||
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
                Count = itemGroup.Count
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

    private static int ApplyDropRateBonus(int baseCount, int bonusPermil)
        => (int)(((long)baseCount * (1000 + bonusPermil) + 999) / 1000);

    // TODO: The current repo has no per-user comeback-grade state, so COMEBACK_GRADE filters cannot be evaluated cleanly yet.
    private sealed record QuestCampaignRow(
        QuestCampaignEffectType EffectType,
        int EffectValue,
        IReadOnlyList<QuestReward> BonusItems,
        IReadOnlyList<QuestCampaignTargetMatch> Targets,
        long StartDatetime,
        long EndDatetime,
        TargetUserStatusType TargetUserStatusType)
    {
        public bool IsActive(long nowMs, TargetUserStatusType userStatus)
            => nowMs >= StartDatetime &&
               nowMs <= EndDatetime &&
               (TargetUserStatusType == TargetUserStatusType.ALL ||
                (TargetUserStatusType != TargetUserStatusType.COMEBACK_GRADE && TargetUserStatusType == userStatus));

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
    EventQuestType EventQuestType = EventQuestType.UNKNOWN);
