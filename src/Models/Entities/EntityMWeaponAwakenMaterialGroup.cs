using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponAwakenMaterialGroup))]
[PrimaryKey(nameof(WeaponAwakenMaterialGroupId), nameof(MaterialId))]
public class EntityMWeaponAwakenMaterialGroup
{
    [Key(0)] public int WeaponAwakenMaterialGroupId { get; set; }

    [Key(1)] public int MaterialId { get; set; }

    [Key(2)] public int Count { get; set; }

    [Key(3)] public int SortOrder { get; set; }
}
