using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMOverrideHitEffectConditionDamageAttribute))]
[PrimaryKey(nameof(OverrideHitEffectConditionId))]
public class EntityMOverrideHitEffectConditionDamageAttribute
{
    [Key(0)] public int OverrideHitEffectConditionId { get; set; }

    [Key(1)] public bool IsExcepting { get; set; }

    [Key(2)] public AttributeType AttributeType { get; set; }
}
