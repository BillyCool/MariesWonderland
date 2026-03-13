using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestCampaignTargetGroup
{
    public int QuestCampaignTargetGroupId { get; set; }

    public int QuestCampaignTargetIndex { get; set; }

    public QuestCampaignTargetType QuestCampaignTargetType { get; set; }

    public int QuestCampaignTargetValue { get; set; }
}
