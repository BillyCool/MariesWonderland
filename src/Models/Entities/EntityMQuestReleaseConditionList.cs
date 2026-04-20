using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestReleaseConditionList))]
public class EntityMQuestReleaseConditionList
{
    [Key(0)] public int QuestReleaseConditionListId { get; set; }

    [Key(1)] public int QuestReleaseConditionGroupId { get; set; }

    [Key(2)] public ConditionOperationType ConditionOperationType { get; set; }
}
