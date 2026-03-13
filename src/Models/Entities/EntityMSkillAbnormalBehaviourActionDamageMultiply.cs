using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalBehaviourActionDamageMultiply
{
    public int SkillAbnormalBehaviourActionId { get; set; }

    public DamageMultiplyDetailType DamageMultiplyDetailType { get; set; }

    public DamageMultiplyTargetType DamageMultiplyTargetType { get; set; }

    public int DamageMultiplyAbnormalDetailId { get; set; }
}
