using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMDeckEntrustCoefficientPartsSeriesBonusCount))]
[PrimaryKey(nameof(PartsSeriesBonusCount))]
public class EntityMDeckEntrustCoefficientPartsSeriesBonusCount
{
    [Key(0)] public int PartsSeriesBonusCount { get; set; }

    [Key(1)] public int CoefficientPermil { get; set; }
}
