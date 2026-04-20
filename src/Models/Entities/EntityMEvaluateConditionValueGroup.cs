using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEvaluateConditionValueGroup))]
public class EntityMEvaluateConditionValueGroup
{
    [Key(0)] public int EvaluateConditionValueGroupId { get; set; }

    [Key(1)] public int GroupIndex { get; set; }

    [Key(2)] public long Value { get; set; }
}
