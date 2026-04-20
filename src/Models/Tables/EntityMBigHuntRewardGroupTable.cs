using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntRewardGroupTable : TableBase<EntityMBigHuntRewardGroup>
{
    private readonly Func<EntityMBigHuntRewardGroup, (int, int)> primaryIndexSelector;

    public EntityMBigHuntRewardGroupTable(EntityMBigHuntRewardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BigHuntRewardGroupId, element.SortOrder);
    }

    public RangeView<EntityMBigHuntRewardGroup> FindRangeByBigHuntRewardGroupIdAndSortOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
