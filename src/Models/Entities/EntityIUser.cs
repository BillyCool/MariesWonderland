using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUser : IUserEntity
{
    public long UserId { get; set; }

    public long PlayerId { get; set; }

    public int OsType { get; set; }

    public PlatformType PlatformType { get; set; }

    public int UserRestrictionType { get; set; }

    public long RegisterDatetime { get; set; }

    public long GameStartDatetime { get; set; }

    public long LatestVersion { get; set; }
}
