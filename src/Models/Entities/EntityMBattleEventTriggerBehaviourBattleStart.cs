using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEventTriggerBehaviourBattleStart))]
[PrimaryKey(nameof(BattleEventTriggerBehaviourId))]
public class EntityMBattleEventTriggerBehaviourBattleStart
{
    [Key(0)] public int BattleEventTriggerBehaviourId { get; set; }

    [Key(1)] public bool TriggerOnBattleRestore { get; set; }
}
