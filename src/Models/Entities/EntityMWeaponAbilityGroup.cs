using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponAbilityGroup))]
[PrimaryKey(nameof(WeaponAbilityGroupId), nameof(SlotNumber))]
public class EntityMWeaponAbilityGroup
{
    [Key(0)] public int WeaponAbilityGroupId { get; set; }

    [Key(1)] public int SlotNumber { get; set; }

    [Key(2)] public int AbilityId { get; set; }

    [Key(3)] public int WeaponAbilityEnhancementMaterialId { get; set; }
}
