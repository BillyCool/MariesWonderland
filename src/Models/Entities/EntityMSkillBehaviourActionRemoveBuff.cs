using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionRemoveBuff
{
    public int SkillBehaviourActionId { get; set; }

    public int RemoveCount { get; set; }

    public BuffType BuffType { get; set; }

    public SkillRemoveBuffFilteringType SkillRemoveBuffFilteringType { get; set; }

    public int SkillRemoveBuffFilteringId { get; set; }

    public SkillRemoveBuffChoosingType SkillRemoveBuffChoosingType { get; set; }
}
