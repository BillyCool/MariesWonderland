using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPartsGroup
{
    public int PartsGroupId { get; set; }

    public int PartsSeriesId { get; set; }

    public int SortOrder { get; set; }

    public int PartsGroupAssetId { get; set; }
}
