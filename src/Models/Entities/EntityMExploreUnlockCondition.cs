using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMExploreUnlockCondition))]
[PrimaryKey(nameof(ExploreUnlockConditionId))]
public class EntityMExploreUnlockCondition
{
    [Key(0)] public int ExploreUnlockConditionId { get; set; }

    [Key(1)] public ExploreUnlockConditionType ExploreUnlockConditionType { get; set; }

    [Key(2)] public int ConditionValue { get; set; }
}
