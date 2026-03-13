using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEnhanceCampaignTargetGroup
{
    public int EnhanceCampaignTargetGroupId { get; set; }

    public int EnhanceCampaignTargetIndex { get; set; }

    public EnhanceCampaignTargetType EnhanceCampaignTargetType { get; set; }

    public int EnhanceCampaignTargetValue { get; set; }
}
