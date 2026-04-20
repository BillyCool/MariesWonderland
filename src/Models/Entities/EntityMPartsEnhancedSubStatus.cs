using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPartsEnhancedSubStatus))]
public class EntityMPartsEnhancedSubStatus
{
    [Key(0)] public int PartsEnhancedId { get; set; }

    [Key(1)] public int StatusIndex { get; set; }

    [Key(2)] public int PartsStatusSubLotteryId { get; set; }

    [Key(3)] public int Level { get; set; }

    [Key(4)] public StatusKindType StatusKindType { get; set; }

    [Key(5)] public StatusCalculationType StatusCalculationType { get; set; }

    [Key(6)] public int FixedStatusChangeValue { get; set; }
}
