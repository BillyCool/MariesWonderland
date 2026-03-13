using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActivationMethod
{
    public int SkillBehaviourActivationMethodId { get; set; }

    public ActivationMethodType ActivationMethodType { get; set; }

    public int SkillBehaviourActivationConditionGroupId { get; set; }
}
