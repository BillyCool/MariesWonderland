using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserConsumableItem
{
    public long UserId { get; set; }

    public int ConsumableItemId { get; set; }

    public int Count { get; set; }

    public long FirstAcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
