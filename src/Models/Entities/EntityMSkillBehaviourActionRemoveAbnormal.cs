using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionRemoveAbnormal
{
    public int SkillBehaviourActionId { get; set; }

    public int TargetPolarityType { get; set; }

    public int SkillRemoveAbnormalTargetAbnormalGroupId { get; set; }

    public RemoveAbnormalTargetType RemoveAbnormalTargetType { get; set; }

    public int RemoveCountUpper { get; set; }

    public int RemoveKindCountUpper { get; set; }
}
