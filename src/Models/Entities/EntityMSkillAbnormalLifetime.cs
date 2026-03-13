using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalLifetime
{
    public int SkillAbnormalLifetimeId { get; set; }

    public int SkillAbnormalLifetimeBehaviourGroupId { get; set; }

    public AbnormalLifetimeBehaviourConditionType AbnormalLifetimeBehaviourConditionType { get; set; }
}
