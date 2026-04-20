using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmickInterval))]
public class EntityMGimmickInterval
{
    [Key(0)] public int GimmickId { get; set; }

    [Key(1)] public int InitialValue { get; set; }

    [Key(2)] public int MaxValue { get; set; }

    [Key(3)] public int IntervalValue { get; set; }
}
