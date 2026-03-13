using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionAdvanceActiveSkillCooltime
{
    public int SkillBehaviourActionId { get; set; }

    public SkillCooltimeAdvanceType SkillCooltimeAdvanceType { get; set; }

    public ActiveSkillType ActiveSkillType { get; set; }

    public int AdvanceValue { get; set; }
}
