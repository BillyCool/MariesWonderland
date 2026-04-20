using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponEnhancedAbility))]
public class EntityMWeaponEnhancedAbility
{
    [Key(0)] public int WeaponEnhancedId { get; set; }

    [Key(1)] public int AbilityId { get; set; }

    [Key(2)] public int Level { get; set; }
}
