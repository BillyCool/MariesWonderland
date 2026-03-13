using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserFacebook
{
    public long UserId { get; set; }

    public long FacebookId { get; set; }

    public long LatestVersion { get; set; }
}
