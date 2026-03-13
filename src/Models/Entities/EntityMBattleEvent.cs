using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleEvent
{
    public int BattleEventId { get; set; }

    public int BattleEventTriggerBehaviourGroupId { get; set; }

    public int BattleEventReceiverBehaviourGroupId { get; set; }
}
