using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserApple : IUserEntity
{
    public long UserId { get; set; }

    public string AppleId { get; set; }

    public long LatestVersion { get; set; }
}
