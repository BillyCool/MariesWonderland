using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeapon))]
public class EntityMWeapon
{
    [Key(0)] public int WeaponId { get; set; }

    [Key(1)] public int WeaponCategoryType { get; set; }

    [Key(2)] public WeaponType WeaponType { get; set; }

    [Key(3)] public int AssetVariationId { get; set; }

    [Key(4)] public RarityType RarityType { get; set; }

    [Key(5)] public AttributeType AttributeType { get; set; }

    [Key(6)] public bool IsRestrictDiscard { get; set; }

    [Key(7)] public int WeaponBaseStatusId { get; set; }

    [Key(8)] public int WeaponStatusCalculationId { get; set; }

    [Key(9)] public int WeaponSkillGroupId { get; set; }

    [Key(10)] public int WeaponAbilityGroupId { get; set; }

    [Key(11)] public int WeaponEvolutionMaterialGroupId { get; set; }

    [Key(12)] public int WeaponEvolutionGrantPossessionGroupId { get; set; }

    [Key(13)] public int WeaponStoryReleaseConditionGroupId { get; set; }

    [Key(14)] public int WeaponSpecificEnhanceId { get; set; }

    [Key(15)] public int WeaponSpecificLimitBreakMaterialGroupId { get; set; }

    [Key(16)] public int CharacterWalkaroundRangeType { get; set; }

    [Key(17)] public bool IsRecyclable { get; set; }
}
