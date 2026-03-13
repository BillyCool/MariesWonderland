using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionExtendBuffCooltime
{
    public int SkillBehaviourActionId { get; set; }

    public ExtendBuffCooltimeBuffType ExtendBuffCooltimeBuffType { get; set; }

    public ExtendBuffCooltimeStatusType ExtendBuffCooltimeStatusType { get; set; }

    public ExtendBuffCooltimeTargetSkillType ExtendBuffCooltimeTargetSkillType { get; set; }

    public ExtendBuffCooltimeExtendType ExtendBuffCooltimeExtendType { get; set; }

    public int ExtendValue { get; set; }
}
