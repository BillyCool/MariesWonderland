using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMFieldEffectDecreasePoint))]
public class EntityMFieldEffectDecreasePoint
{
    [Key(0)] public int WeaponId { get; set; }

    [Key(1)] public int FieldEffectAbilityId { get; set; }

    [Key(2)] public int DecreasePoint { get; set; }
}
