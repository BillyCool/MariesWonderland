using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponSkillEnhancementMaterial))]
public class EntityMWeaponSkillEnhancementMaterial
{
    [Key(0)] public int WeaponSkillEnhancementMaterialId { get; set; }

    [Key(1)] public int SkillLevel { get; set; }

    [Key(2)] public int MaterialId { get; set; }

    [Key(3)] public int Count { get; set; }

    [Key(4)] public int SortOrder { get; set; }
}
