using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserApple : IUserEntity
{
    public long UserId { get; set; }

    public string AppleId { get; set; }

    public long LatestVersion { get; set; }
}
