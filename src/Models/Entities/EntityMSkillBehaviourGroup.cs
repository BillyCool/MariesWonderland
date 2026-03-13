using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourGroup
{
    public int SkillBehaviourGroupId { get; set; }

    public int SkillBehaviourId { get; set; }

    public int SkillBehaviourIndex { get; set; }

    public int TargetSelectorIndex { get; set; }

    public int SkillHitStartIndex { get; set; }

    public int SkillHitEndIndex { get; set; }
}
