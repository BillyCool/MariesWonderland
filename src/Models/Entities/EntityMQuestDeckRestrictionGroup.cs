using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestDeckRestrictionGroup))]
public class EntityMQuestDeckRestrictionGroup
{
    [Key(0)] public int QuestDeckRestrictionGroupId { get; set; }

    [Key(1)] public int SlotNumber { get; set; }

    [Key(2)] public QuestDeckRestrictionType QuestDeckRestrictionType { get; set; }

    [Key(3)] public int RestrictionValue { get; set; }
}
