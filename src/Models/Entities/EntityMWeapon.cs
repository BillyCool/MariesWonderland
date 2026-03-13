using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeapon
{
    public int WeaponId { get; set; }

    public int WeaponCategoryType { get; set; }

    public WeaponType WeaponType { get; set; }

    public int AssetVariationId { get; set; }

    public RarityType RarityType { get; set; }

    public AttributeType AttributeType { get; set; }

    public bool IsRestrictDiscard { get; set; }

    public int WeaponBaseStatusId { get; set; }

    public int WeaponStatusCalculationId { get; set; }

    public int WeaponSkillGroupId { get; set; }

    public int WeaponAbilityGroupId { get; set; }

    public int WeaponEvolutionMaterialGroupId { get; set; }

    public int WeaponEvolutionGrantPossessionGroupId { get; set; }

    public int WeaponStoryReleaseConditionGroupId { get; set; }

    public int WeaponSpecificEnhanceId { get; set; }

    public int WeaponSpecificLimitBreakMaterialGroupId { get; set; }

    public int CharacterWalkaroundRangeType { get; set; }

    public bool IsRecyclable { get; set; }
}
