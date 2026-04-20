using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleEventReceiverBehaviourHudActSequenceTable : TableBase<EntityMBattleEventReceiverBehaviourHudActSequence>
{
    private readonly Func<EntityMBattleEventReceiverBehaviourHudActSequence, int> primaryIndexSelector;

    public EntityMBattleEventReceiverBehaviourHudActSequenceTable(EntityMBattleEventReceiverBehaviourHudActSequence[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BattleEventReceiverBehaviourId;
    }

    public EntityMBattleEventReceiverBehaviourHudActSequence FindByBattleEventReceiverBehaviourId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
