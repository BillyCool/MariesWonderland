using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalBehaviour
{
    public int SkillAbnormalBehaviourId { get; set; }

    public AbnormalBehaviourActionType AbnormalBehaviourActionType { get; set; }

    public AbnormalBehaviourActivationMethodType AbnormalBehaviourActivationMethodType { get; set; }

    public AbnormalBehaviourDeactivationMethodType AbnormalBehaviourDeactivationMethodType { get; set; }

    public int SkillAbnormalBehaviourActionId { get; set; }
}
