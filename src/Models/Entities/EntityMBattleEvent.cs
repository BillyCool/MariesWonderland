using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEvent))]
[PrimaryKey(nameof(BattleEventId))]
public class EntityMBattleEvent
{
    [Key(0)] public int BattleEventId { get; set; }

    [Key(1)] public int BattleEventTriggerBehaviourGroupId { get; set; }

    [Key(2)] public int BattleEventReceiverBehaviourGroupId { get; set; }
}
