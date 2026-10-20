using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEventTriggerBehaviourWaveStart))]
[PrimaryKey(nameof(BattleEventTriggerBehaviourId))]
public class EntityMBattleEventTriggerBehaviourWaveStart
{
    [Key(0)] public int BattleEventTriggerBehaviourId { get; set; }

    [Key(1)] public int WaveNumber { get; set; }
}
