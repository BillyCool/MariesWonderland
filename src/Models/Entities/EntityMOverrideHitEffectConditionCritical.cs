using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMOverrideHitEffectConditionCritical))]
public class EntityMOverrideHitEffectConditionCritical
{
    [Key(0)] public int OverrideHitEffectConditionId { get; set; }

    [Key(1)] public bool IsCritical { get; set; }
}
