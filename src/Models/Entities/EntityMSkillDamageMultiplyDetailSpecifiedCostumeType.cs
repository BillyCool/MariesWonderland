using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillDamageMultiplyDetailSpecifiedCostumeType
{
    public int SkillDamageMultiplyDetailId { get; set; }

    public DamageMultiplySpecifiedCostumeConditionTargetType SpecifiedCostumeConditionTargetType { get; set; }

    public int TargetSpecifiedCostumeGroupId { get; set; }

    public int DamageMultiplyCoefficientValuePermil { get; set; }
}
