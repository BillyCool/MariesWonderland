using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponEvolutionGroup))]
public class EntityMWeaponEvolutionGroup
{
    [Key(0)] public int WeaponEvolutionGroupId { get; set; }

    [Key(1)] public int EvolutionOrder { get; set; }

    [Key(2)] public int WeaponId { get; set; }
}
