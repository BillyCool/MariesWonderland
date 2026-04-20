using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLimitContentDeckRestrictionTarget))]
public class EntityMEventQuestLimitContentDeckRestrictionTarget
{
    [Key(0)] public int EventQuestLimitContentDeckRestrictionTargetId { get; set; }

    [Key(1)] public LimitContentDeckRestrictionType LimitContentDeckRestrictionType { get; set; }
}
