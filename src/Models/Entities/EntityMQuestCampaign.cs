using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestCampaign))]
public class EntityMQuestCampaign
{
    [Key(0)] public int QuestCampaignId { get; set; }

    [Key(1)] public int QuestCampaignTargetGroupId { get; set; }

    [Key(2)] public int QuestCampaignEffectGroupId { get; set; }

    [Key(3)] public long StartDatetime { get; set; }

    [Key(4)] public long EndDatetime { get; set; }

    [Key(5)] public TargetUserStatusType TargetUserStatusType { get; set; }

    [Key(6)] public int SortOrder { get; set; }
}
