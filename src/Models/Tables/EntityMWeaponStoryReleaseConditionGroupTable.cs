using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponStoryReleaseConditionGroupTable : TableBase<EntityMWeaponStoryReleaseConditionGroup>
{
    private readonly Func<EntityMWeaponStoryReleaseConditionGroup, (int, int)> primaryIndexSelector;

    public EntityMWeaponStoryReleaseConditionGroupTable(EntityMWeaponStoryReleaseConditionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponStoryReleaseConditionGroupId, element.StoryIndex);
    }

    public EntityMWeaponStoryReleaseConditionGroup FindByWeaponStoryReleaseConditionGroupIdAndStoryIndex(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);

    public RangeView<EntityMWeaponStoryReleaseConditionGroup> FindRangeByWeaponStoryReleaseConditionGroupIdAndStoryIndex(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
