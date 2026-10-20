using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEventTriggerBehaviourGroup))]
[PrimaryKey(nameof(BattleEventTriggerBehaviourGroupId), nameof(BattleEventTriggerBehaviourType), nameof(BattleEventTriggerBehaviourId))]
public class EntityMBattleEventTriggerBehaviourGroup
{
    [Key(0)] public int BattleEventTriggerBehaviourGroupId { get; set; }

    [Key(1)] public int BattleEventTriggerBehaviourType { get; set; }

    [Key(2)] public int BattleEventTriggerBehaviourId { get; set; }
}
