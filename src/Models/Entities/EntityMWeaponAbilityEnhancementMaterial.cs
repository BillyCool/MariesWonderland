using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponAbilityEnhancementMaterial
{
    public int WeaponAbilityEnhancementMaterialId { get; set; }

    public int AbilityLevel { get; set; }

    public int MaterialId { get; set; }

    public int Count { get; set; }

    public int SortOrder { get; set; }
}
