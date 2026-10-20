using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponSkillEnhancementMaterial))]
[PrimaryKey(nameof(WeaponSkillEnhancementMaterialId), nameof(SkillLevel), nameof(MaterialId))]
public class EntityMWeaponSkillEnhancementMaterial
{
    [Key(0)] public int WeaponSkillEnhancementMaterialId { get; set; }

    [Key(1)] public int SkillLevel { get; set; }

    [Key(2)] public int MaterialId { get; set; }

    [Key(3)] public int Count { get; set; }

    [Key(4)] public int SortOrder { get; set; }
}
