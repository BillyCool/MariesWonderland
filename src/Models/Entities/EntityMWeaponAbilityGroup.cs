using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponAbilityGroup
{
    public int WeaponAbilityGroupId { get; set; }

    public int SlotNumber { get; set; }

    public int AbilityId { get; set; }

    public int WeaponAbilityEnhancementMaterialId { get; set; }
}
