using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserImportantItem
{
    public long UserId { get; set; }

    public int ImportantItemId { get; set; }

    public int Count { get; set; }

    public long FirstAcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
