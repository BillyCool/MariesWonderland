using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEnhanceCampaign
{
    public int EnhanceCampaignId { get; set; }

    public int EnhanceCampaignTargetGroupId { get; set; }

    public EnhanceCampaignEffectType EnhanceCampaignEffectType { get; set; }

    public int EnhanceCampaignEffectValue { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public TargetUserStatusType TargetUserStatusType { get; set; }

    public int SortOrder { get; set; }
}
