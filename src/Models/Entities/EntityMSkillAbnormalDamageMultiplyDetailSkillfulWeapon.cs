using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalDamageMultiplyDetailSkillfulWeapon
{
    public int DamageMultiplyAbnormalDetailId { get; set; }

    public int ConditionTargetType { get; set; }

    public WeaponType WeaponType { get; set; }

    public bool IsSkillfulMainWeapon { get; set; }

    public int DamageMultiplyCoefficientValuePermil { get; set; }
}
