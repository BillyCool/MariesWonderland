using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponAwakenEffectGroup))]
[PrimaryKey(nameof(WeaponAwakenEffectGroupId), nameof(WeaponAwakenEffectType))]
public class EntityMWeaponAwakenEffectGroup
{
    [Key(0)] public int WeaponAwakenEffectGroupId { get; set; }

    [Key(1)] public int WeaponAwakenEffectType { get; set; }

    [Key(2)] public int WeaponAwakenEffectId { get; set; }
}
