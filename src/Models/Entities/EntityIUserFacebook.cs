using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserFacebook : IUserEntity
{
    public long UserId { get; set; }

    public long FacebookId { get; set; }

    public long LatestVersion { get; set; }
}
