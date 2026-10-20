using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLimitContentDeckRestrictionTarget))]
[PrimaryKey(nameof(EventQuestLimitContentDeckRestrictionTargetId), nameof(LimitContentDeckRestrictionType))]
public class EntityMEventQuestLimitContentDeckRestrictionTarget
{
    [Key(0)] public int EventQuestLimitContentDeckRestrictionTargetId { get; set; }

    [Key(1)] public LimitContentDeckRestrictionType LimitContentDeckRestrictionType { get; set; }
}
