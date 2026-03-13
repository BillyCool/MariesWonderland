using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalBehaviourActionRecovery
{
    public int SkillAbnormalBehaviourActionId { get; set; }

    public AbnormalBehaviourRecoveryType AbnormalBehaviourRecoveryType { get; set; }

    public int Value { get; set; }

    public int Upper { get; set; }
}
