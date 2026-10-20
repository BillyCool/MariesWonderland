using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponAwakenStatusUpGroup))]
[PrimaryKey(nameof(WeaponAwakenStatusUpGroupId), nameof(StatusKindType), nameof(StatusCalculationType))]
public class EntityMWeaponAwakenStatusUpGroup
{
    [Key(0)] public int WeaponAwakenStatusUpGroupId { get; set; }

    [Key(1)] public StatusKindType StatusKindType { get; set; }

    [Key(2)] public StatusCalculationType StatusCalculationType { get; set; }

    [Key(3)] public int EffectValue { get; set; }
}
