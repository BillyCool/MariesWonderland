using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponFieldEffectDecreasePoint
{
    public int WeaponId { get; set; }

    public int RelationIndex { get; set; }

    public int FieldEffectGroupId { get; set; }

    public int FieldEffectAbilityId { get; set; }

    public int DecreasePoint { get; set; }

    public int WeaponAbilityId { get; set; }

    public bool IsWeaponAwaken { get; set; }

    public int AutoOrganizationCoefficientPermil { get; set; }
}
