using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMAbilityBehaviourGroupTable : TableBase<EntityMAbilityBehaviourGroup>
{
    private readonly Func<EntityMAbilityBehaviourGroup, (int, int)> primaryIndexSelector;

    public EntityMAbilityBehaviourGroupTable(EntityMAbilityBehaviourGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.AbilityBehaviourGroupId, element.AbilityBehaviourIndex);
    }

    public RangeView<EntityMAbilityBehaviourGroup> FindRangeByAbilityBehaviourGroupIdAndAbilityBehaviourIndex(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
