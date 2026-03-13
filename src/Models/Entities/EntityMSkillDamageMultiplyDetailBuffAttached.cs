using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillDamageMultiplyDetailBuffAttached
{
    public int SkillDamageMultiplyDetailId { get; set; }

    public DamageMultiplyBuffAttachedConditionTargetType BuffAttachedTargetType { get; set; }

    public DamageMultiplyBuffAttachedTargetBuffType TargetBuffType { get; set; }

    public DamageMultiplyBuffAttachedTargetStatusKindType TargetStatusKindType { get; set; }

    public int DamageMultiplyCoefficientValuePermil { get; set; }
}
