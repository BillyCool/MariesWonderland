using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestPickupRewardGroup))]
public class EntityMQuestPickupRewardGroup
{
    [Key(0)] public int QuestPickupRewardGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int BattleDropRewardId { get; set; }
}
