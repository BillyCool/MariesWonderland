using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponStoryReleaseConditionGroup
{
    public int WeaponStoryReleaseConditionGroupId { get; set; }

    public int StoryIndex { get; set; }

    public WeaponStoryReleaseConditionType WeaponStoryReleaseConditionType { get; set; }

    public int ConditionValue { get; set; }

    public int WeaponStoryReleaseConditionOperationGroupId { get; set; }
}
