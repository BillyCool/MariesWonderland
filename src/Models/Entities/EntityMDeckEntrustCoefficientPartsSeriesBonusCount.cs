using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMDeckEntrustCoefficientPartsSeriesBonusCount))]
public class EntityMDeckEntrustCoefficientPartsSeriesBonusCount
{
    [Key(0)] public int PartsSeriesBonusCount { get; set; }

    [Key(1)] public int CoefficientPermil { get; set; }
}
