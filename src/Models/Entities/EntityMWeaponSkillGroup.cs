using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponSkillGroup
{
    public int WeaponSkillGroupId { get; set; }

    public int SlotNumber { get; set; }

    public int SkillId { get; set; }

    public int WeaponSkillEnhancementMaterialId { get; set; }
}
