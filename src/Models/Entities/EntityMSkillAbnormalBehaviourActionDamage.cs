using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalBehaviourActionDamage
{
    public int SkillAbnormalBehaviourActionId { get; set; }

    public AbnormalBehaviourDamageType AbnormalBehaviourDamageType { get; set; }

    public int Power { get; set; }
}
