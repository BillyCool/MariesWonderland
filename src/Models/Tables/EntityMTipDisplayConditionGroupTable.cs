using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMTipDisplayConditionGroupTable : TableBase<EntityMTipDisplayConditionGroup>
{
    private readonly Func<EntityMTipDisplayConditionGroup, (int, int)> primaryIndexSelector;

    public EntityMTipDisplayConditionGroupTable(EntityMTipDisplayConditionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.TipDisplayConditionGroupId, element.SortOrder);
    }

    public RangeView<EntityMTipDisplayConditionGroup> FindRangeByTipDisplayConditionGroupIdAndSortOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
