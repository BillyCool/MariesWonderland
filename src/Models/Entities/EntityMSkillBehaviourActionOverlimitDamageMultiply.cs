using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionOverlimitDamageMultiply
{
    public int SkillBehaviourActionId { get; set; }

    public DamageMultiplyDetailType DamageMultiplyDetailType { get; set; }

    public int SkillDamageMultiplyDetailId { get; set; }

    public DamageMultiplyTargetType DamageMultiplyTargetType { get; set; }
}
