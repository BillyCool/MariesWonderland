using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillCasttimeBehaviourActionOnSkillDamageCondition
{
    public int SkillCasttimeBehaviourActionId { get; set; }

    public int SkillCasttimeUpdateValue { get; set; }

    public SkillCasttimeAdvanceType SkillCasttimeAdvanceType { get; set; }

    public int DamageCompareType { get; set; }

    public int DamageConditionValue { get; set; }
}
