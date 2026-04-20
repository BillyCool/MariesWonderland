using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMissionUnlockCondition))]
public class EntityMMissionUnlockCondition
{
    [Key(0)] public int MissionUnlockConditionId { get; set; }

    [Key(1)] public MissionUnlockConditionType MissionUnlockConditionType { get; set; }

    [Key(2)] public int ConditionValue { get; set; }
}
