using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalBehaviourActionAbnormalResistance
{
    public int SkillAbnormalBehaviourActionId { get; set; }

    public AbnormalResistancePolarityType AbnormalResistancePolarityType { get; set; }

    public int AbnormalResistanceSkillAbnormalTypeId { get; set; }

    public int BlockProbabilityPermil { get; set; }
}
