using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEvent))]
public class EntityMBattleEvent
{
    [Key(0)] public int BattleEventId { get; set; }

    [Key(1)] public int BattleEventTriggerBehaviourGroupId { get; set; }

    [Key(2)] public int BattleEventReceiverBehaviourGroupId { get; set; }
}
