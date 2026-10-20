using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCatalogWeapon))]
[PrimaryKey(nameof(WeaponId))]
public class EntityMCatalogWeapon
{
    [Key(0)] public int WeaponId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int CatalogTermId { get; set; }
}
