using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalBehaviourActionModifyHateValue
{
    public int SkillAbnormalBehaviourActionId { get; set; }

    public HateValueCalculationType HateValueCalculationType { get; set; }

    public int ModifyValue { get; set; }
}
