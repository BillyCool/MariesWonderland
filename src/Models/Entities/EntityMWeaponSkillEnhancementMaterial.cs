using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponSkillEnhancementMaterial
{
    public int WeaponSkillEnhancementMaterialId { get; set; }

    public int SkillLevel { get; set; }

    public int MaterialId { get; set; }

    public int Count { get; set; }

    public int SortOrder { get; set; }
}
