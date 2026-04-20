using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMFieldEffectBlessRelation))]
public class EntityMFieldEffectBlessRelation
{
    [Key(0)] public int FieldEffectGroupId { get; set; }

    [Key(1)] public int FieldEffectBlessRelationIndex { get; set; }

    [Key(2)] public int WeaponId { get; set; }
}
