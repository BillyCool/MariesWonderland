using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserMaterial
{
    public long UserId { get; set; }

    public int MaterialId { get; set; }

    public int Count { get; set; }

    public long FirstAcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
