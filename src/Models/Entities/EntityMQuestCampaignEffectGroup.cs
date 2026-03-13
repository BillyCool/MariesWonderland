using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestCampaignEffectGroup
{
    public int QuestCampaignEffectGroupId { get; set; }

    public QuestCampaignEffectType QuestCampaignEffectType { get; set; }

    public int QuestCampaignEffectValue { get; set; }

    public int QuestCampaignTargetItemGroupId { get; set; }
}
