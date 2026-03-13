using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalBehaviourActionTurnRestriction
{
    public int SkillBehaviourActionId { get; set; }

    public int TurnRestrictionProbabilityPermil { get; set; }

    public AbnormalBehaviourTurnRestrictionSkillType AbnormalBehaviourTurnRestrictionSkillType { get; set; }
}
