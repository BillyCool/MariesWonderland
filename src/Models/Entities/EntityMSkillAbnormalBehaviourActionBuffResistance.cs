using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalBehaviourActionBuffResistance
{
    public int SkillAbnormalBehaviourActionId { get; set; }

    public BuffResistanceType BuffResistanceType { get; set; }

    public BuffResistanceStatusKindType BuffResistanceStatusKindType { get; set; }

    public int BlockProbabilityPermil { get; set; }
}
