using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourActivationConditionGroupTable : TableBase<EntityMSkillBehaviourActivationConditionGroup>
{
    private readonly Func<EntityMSkillBehaviourActivationConditionGroup, (int, int)> primaryIndexSelector;

    public EntityMSkillBehaviourActivationConditionGroupTable(EntityMSkillBehaviourActivationConditionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.SkillBehaviourActivationConditionGroupId, element.ConditionCheckOrder);
    }

    public RangeView<EntityMSkillBehaviourActivationConditionGroup> FindRangeBySkillBehaviourActivationConditionGroupIdAndConditionCheckOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
