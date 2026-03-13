using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPartsSeries
{
    public int PartsSeriesId { get; set; }

    public int PartsSeriesBonusAbilityGroupId { get; set; }

    public int PartsSeriesAssetId { get; set; }

    public long ListSettingDisplayStartDatetime { get; set; }
}
