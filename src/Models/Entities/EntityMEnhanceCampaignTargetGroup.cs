using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEnhanceCampaignTargetGroup))]
[PrimaryKey(nameof(EnhanceCampaignTargetGroupId), nameof(EnhanceCampaignTargetIndex))]
public class EntityMEnhanceCampaignTargetGroup
{
    [Key(0)] public int EnhanceCampaignTargetGroupId { get; set; }

    [Key(1)] public int EnhanceCampaignTargetIndex { get; set; }

    [Key(2)] public EnhanceCampaignTargetType EnhanceCampaignTargetType { get; set; }

    [Key(3)] public int EnhanceCampaignTargetValue { get; set; }
}
