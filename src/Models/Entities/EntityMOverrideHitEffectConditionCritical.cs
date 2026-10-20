using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMOverrideHitEffectConditionCritical))]
[PrimaryKey(nameof(OverrideHitEffectConditionId))]
public class EntityMOverrideHitEffectConditionCritical
{
    [Key(0)] public int OverrideHitEffectConditionId { get; set; }

    [Key(1)] public bool IsCritical { get; set; }
}
