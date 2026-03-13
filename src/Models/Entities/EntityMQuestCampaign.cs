using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestCampaign
{
    public int QuestCampaignId { get; set; }

    public int QuestCampaignTargetGroupId { get; set; }

    public int QuestCampaignEffectGroupId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public TargetUserStatusType TargetUserStatusType { get; set; }

    public int SortOrder { get; set; }
}
