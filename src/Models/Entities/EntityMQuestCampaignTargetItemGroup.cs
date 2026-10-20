using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestCampaignTargetItemGroup))]
[PrimaryKey(nameof(QuestCampaignTargetItemGroupId), nameof(TargetIndex))]
public class EntityMQuestCampaignTargetItemGroup
{
    [Key(0)] public int QuestCampaignTargetItemGroupId { get; set; }

    [Key(1)] public int TargetIndex { get; set; }

    [Key(2)] public PossessionType PossessionType { get; set; }

    [Key(3)] public int PossessionId { get; set; }

    [Key(4)] public int Count { get; set; }
}
