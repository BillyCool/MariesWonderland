using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEventReceiverBehaviourHudActSequence))]
[PrimaryKey(nameof(BattleEventReceiverBehaviourId))]
public class EntityMBattleEventReceiverBehaviourHudActSequence
{
    [Key(0)] public int BattleEventReceiverBehaviourId { get; set; }

    [Key(1)] public int HudActSequenceId { get; set; }
}
