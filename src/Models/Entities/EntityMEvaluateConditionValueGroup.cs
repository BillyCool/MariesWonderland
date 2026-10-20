using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEvaluateConditionValueGroup))]
[PrimaryKey(nameof(EvaluateConditionValueGroupId), nameof(GroupIndex))]
public class EntityMEvaluateConditionValueGroup
{
    [Key(0)] public int EvaluateConditionValueGroupId { get; set; }

    [Key(1)] public int GroupIndex { get; set; }

    [Key(2)] public long Value { get; set; }
}
