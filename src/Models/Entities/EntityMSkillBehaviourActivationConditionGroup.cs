using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActivationConditionGroup
{
    public int SkillBehaviourActivationConditionGroupId { get; set; }

    public int ConditionCheckOrder { get; set; }

    public SkillBehaviourActivationConditionType SkillBehaviourActivationConditionType { get; set; }

    public int SkillBehaviourActivationConditionId { get; set; }
}
