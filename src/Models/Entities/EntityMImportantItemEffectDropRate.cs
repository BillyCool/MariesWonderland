using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMImportantItemEffectDropRate))]
[PrimaryKey(nameof(ImportantItemEffectDropRateId))]
public class EntityMImportantItemEffectDropRate
{
    [Key(0)] public int ImportantItemEffectDropRateId { get; set; }

    [Key(1)] public int RatePermil { get; set; }

    [Key(2)] public int ImportantItemEffectTargetQuestGroupId { get; set; }

    [Key(3)] public int ImportantItemEffectTargetItemGroupId { get; set; }
}
