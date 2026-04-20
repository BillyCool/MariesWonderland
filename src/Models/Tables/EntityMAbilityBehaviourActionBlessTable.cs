using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMAbilityBehaviourActionBlessTable : TableBase<EntityMAbilityBehaviourActionBless>
{
    private readonly Func<EntityMAbilityBehaviourActionBless, int> primaryIndexSelector;

    public EntityMAbilityBehaviourActionBlessTable(EntityMAbilityBehaviourActionBless[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.AbilityBehaviourActionId;
    }

    public EntityMAbilityBehaviourActionBless FindByAbilityBehaviourActionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
