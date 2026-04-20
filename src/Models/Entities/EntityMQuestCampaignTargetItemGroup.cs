using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestCampaignTargetItemGroup))]
public class EntityMQuestCampaignTargetItemGroup
{
    [Key(0)] public int QuestCampaignTargetItemGroupId { get; set; }

    [Key(1)] public int TargetIndex { get; set; }

    [Key(2)] public PossessionType PossessionType { get; set; }

    [Key(3)] public int PossessionId { get; set; }

    [Key(4)] public int Count { get; set; }
}
