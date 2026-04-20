using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEventTriggerBehaviourBattleStart))]
public class EntityMBattleEventTriggerBehaviourBattleStart
{
    [Key(0)] public int BattleEventTriggerBehaviourId { get; set; }

    [Key(1)] public bool TriggerOnBattleRestore { get; set; }
}
