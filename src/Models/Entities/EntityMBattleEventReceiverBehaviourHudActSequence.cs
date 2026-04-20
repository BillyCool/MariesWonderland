using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEventReceiverBehaviourHudActSequence))]
public class EntityMBattleEventReceiverBehaviourHudActSequence
{
    [Key(0)] public int BattleEventReceiverBehaviourId { get; set; }

    [Key(1)] public int HudActSequenceId { get; set; }
}
