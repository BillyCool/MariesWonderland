using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMissionUnlockCondition))]
[PrimaryKey(nameof(MissionUnlockConditionId))]
public class EntityMMissionUnlockCondition
{
    [Key(0)] public int MissionUnlockConditionId { get; set; }

    [Key(1)] public MissionUnlockConditionType MissionUnlockConditionType { get; set; }

    [Key(2)] public int ConditionValue { get; set; }
}
