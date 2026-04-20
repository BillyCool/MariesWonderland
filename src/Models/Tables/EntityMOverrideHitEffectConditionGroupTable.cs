using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMOverrideHitEffectConditionGroupTable : TableBase<EntityMOverrideHitEffectConditionGroup>
{
    private readonly Func<EntityMOverrideHitEffectConditionGroup, (int, int)> primaryIndexSelector;

    public EntityMOverrideHitEffectConditionGroupTable(EntityMOverrideHitEffectConditionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.OverrideHitEffectConditionGroupId, element.ConditionIndex);
    }

    public RangeView<EntityMOverrideHitEffectConditionGroup> FindRangeByOverrideHitEffectConditionGroupIdAndConditionIndex(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
