using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponStoryReleaseConditionOperation
{
    public int WeaponStoryReleaseConditionOperationGroupId { get; set; }

    public int SortOrder { get; set; }

    public WeaponStoryReleaseConditionType WeaponStoryReleaseConditionType { get; set; }

    public int ConditionValue { get; set; }
}
