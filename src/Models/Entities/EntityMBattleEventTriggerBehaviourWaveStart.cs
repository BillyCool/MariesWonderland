using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEventTriggerBehaviourWaveStart))]
public class EntityMBattleEventTriggerBehaviourWaveStart
{
    [Key(0)] public int BattleEventTriggerBehaviourId { get; set; }

    [Key(1)] public int WaveNumber { get; set; }
}
