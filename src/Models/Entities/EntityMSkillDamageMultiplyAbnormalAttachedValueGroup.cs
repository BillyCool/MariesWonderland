using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillDamageMultiplyAbnormalAttachedValueGroup
{
    public int SkillDamageMultiplyAbnormalAttachedValueGroupId { get; set; }

    public int SkillDamageMultiplyAbnormalAttachedValueGroupIndex { get; set; }

    public DamageMultiplyAbnormalAttachedPolarityConditionType PolarityConditionType { get; set; }

    public int SkillAbnormalTypeIdCondition { get; set; }

    public DamageMultiplyAbnormalAttachedTargetType TargetType { get; set; }

    public int DamageMultiplyCoefficientValuePermil { get; set; }
}
