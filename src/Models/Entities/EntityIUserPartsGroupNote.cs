using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserPartsGroupNote
{
    public long UserId { get; set; }

    public int PartsGroupId { get; set; }

    public long FirstAcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
