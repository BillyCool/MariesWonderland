using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPartsEnhanced))]
public class EntityMPartsEnhanced
{
    [Key(0)] public int PartsEnhancedId { get; set; }

    [Key(1)] public int PartsId { get; set; }

    [Key(2)] public int PartsStatusMainId { get; set; }

    [Key(3)] public int Level { get; set; }

    [Key(4)] public bool IsRandomSubStatusCount { get; set; }

    [Key(5)] public int SubStatusCount { get; set; }
}
