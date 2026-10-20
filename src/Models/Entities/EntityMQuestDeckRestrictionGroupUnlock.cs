using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestDeckRestrictionGroupUnlock))]
[PrimaryKey(nameof(QuestDeckRestrictionGroupId))]
public class EntityMQuestDeckRestrictionGroupUnlock
{
    [Key(0)] public int QuestDeckRestrictionGroupId { get; set; }

    [Key(1)] public int UnlockEvaluateConditionId { get; set; }
}
