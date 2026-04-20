using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleEventTriggerBehaviourBattleStartTable : TableBase<EntityMBattleEventTriggerBehaviourBattleStart>
{
    private readonly Func<EntityMBattleEventTriggerBehaviourBattleStart, int> primaryIndexSelector;

    public EntityMBattleEventTriggerBehaviourBattleStartTable(EntityMBattleEventTriggerBehaviourBattleStart[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BattleEventTriggerBehaviourId;
    }

    public EntityMBattleEventTriggerBehaviourBattleStart FindByBattleEventTriggerBehaviourId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
