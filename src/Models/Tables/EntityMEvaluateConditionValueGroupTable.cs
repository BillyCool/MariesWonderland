using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEvaluateConditionValueGroupTable : TableBase<EntityMEvaluateConditionValueGroup>
{
    private readonly Func<EntityMEvaluateConditionValueGroup, (int, int)> primaryIndexSelector;

    public EntityMEvaluateConditionValueGroupTable(EntityMEvaluateConditionValueGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EvaluateConditionValueGroupId, element.GroupIndex);
    }

    public EntityMEvaluateConditionValueGroup FindByEvaluateConditionValueGroupIdAndGroupIndex(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);

    public RangeView<EntityMEvaluateConditionValueGroup> FindRangeByEvaluateConditionValueGroupIdAndGroupIndex(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
