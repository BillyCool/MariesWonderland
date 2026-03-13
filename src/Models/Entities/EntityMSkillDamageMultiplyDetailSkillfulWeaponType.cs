using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillDamageMultiplyDetailSkillfulWeaponType
{
    public int SkillDamageMultiplyDetailId { get; set; }

    public DamageMultiplySkillfulWeaponConditionTargetType ConditionTargetType { get; set; }

    public WeaponType WeaponType { get; set; }

    public bool IsSkillfulMainWeapon { get; set; }

    public int DamageMultiplyCoefficientValuePermil { get; set; }
}
