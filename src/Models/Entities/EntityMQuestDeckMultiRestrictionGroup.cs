using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestDeckMultiRestrictionGroup))]
public class EntityMQuestDeckMultiRestrictionGroup
{
    [Key(0)] public int QuestDeckMultiRestrictionGroupId { get; set; }

    [Key(1)] public int GroupIndex { get; set; }

    [Key(2)] public int RestrictionValue { get; set; }
}
