using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPartsSeries))]
public class EntityMPartsSeries
{
    [Key(0)] public int PartsSeriesId { get; set; }

    [Key(1)] public int PartsSeriesBonusAbilityGroupId { get; set; }

    [Key(2)] public int PartsSeriesAssetId { get; set; }

    [Key(3)] public long ListSettingDisplayStartDatetime { get; set; }
}
