using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponStoryReleaseConditionOperationTable : TableBase<EntityMWeaponStoryReleaseConditionOperation>
{
    private readonly Func<EntityMWeaponStoryReleaseConditionOperation, (int, int)> primaryIndexSelector;

    public EntityMWeaponStoryReleaseConditionOperationTable(EntityMWeaponStoryReleaseConditionOperation[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponStoryReleaseConditionOperationGroupId, element.SortOrder);
    }

    public RangeView<EntityMWeaponStoryReleaseConditionOperation> FindRangeByWeaponStoryReleaseConditionOperationGroupIdAndSortOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
