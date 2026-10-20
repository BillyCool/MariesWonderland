using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeActiveSkillEnhancementMaterial))]
[PrimaryKey(nameof(CostumeActiveSkillEnhancementMaterialId), nameof(SkillLevel), nameof(MaterialId))]
public class EntityMCostumeActiveSkillEnhancementMaterial
{
    [Key(0)] public int CostumeActiveSkillEnhancementMaterialId { get; set; }

    [Key(1)] public int SkillLevel { get; set; }

    [Key(2)] public int MaterialId { get; set; }

    [Key(3)] public int Count { get; set; }

    [Key(4)] public int SortOrder { get; set; }
}
