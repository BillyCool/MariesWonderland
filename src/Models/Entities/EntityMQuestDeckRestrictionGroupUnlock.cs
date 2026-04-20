using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestDeckRestrictionGroupUnlock))]
public class EntityMQuestDeckRestrictionGroupUnlock
{
    [Key(0)] public int QuestDeckRestrictionGroupId { get; set; }

    [Key(1)] public int UnlockEvaluateConditionId { get; set; }
}
