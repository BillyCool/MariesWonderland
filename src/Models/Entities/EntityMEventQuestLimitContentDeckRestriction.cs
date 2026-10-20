using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLimitContentDeckRestriction))]
[PrimaryKey(nameof(EventQuestLimitContentDeckRestrictionId), nameof(GroupIndex))]
public class EntityMEventQuestLimitContentDeckRestriction
{
    [Key(0)] public int EventQuestLimitContentDeckRestrictionId { get; set; }

    [Key(1)] public int GroupIndex { get; set; }

    [Key(2)] public int EventQuestLimitContentDeckRestrictionTargetId { get; set; }

    [Key(3)] public long StartDatetime { get; set; }

    [Key(4)] public long EndDatetime { get; set; }
}
