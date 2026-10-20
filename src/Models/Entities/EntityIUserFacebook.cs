using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserFacebook : IUserEntity
{
    public long UserId { get; set; }

    public long FacebookId { get; set; }

    public long LatestVersion { get; set; }
}
