using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestFirstClearRewardGroup))]
public class EntityMQuestFirstClearRewardGroup
{
    [Key(0)] public int QuestFirstClearRewardGroupId { get; set; }

    [Key(1)] public QuestFirstClearRewardType QuestFirstClearRewardType { get; set; }

    [Key(2)] public int SortOrder { get; set; }

    [Key(3)] public PossessionType PossessionType { get; set; }

    [Key(4)] public int PossessionId { get; set; }

    [Key(5)] public int Count { get; set; }

    [Key(6)] public bool IsPickup { get; set; }
}
