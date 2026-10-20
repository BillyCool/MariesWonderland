using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(LimitedOpenTargetType), nameof(TargetId))]
public class EntityIUserLimitedOpen : IUserEntity
{
    public long UserId { get; set; }

    public LimitedOpenTargetType LimitedOpenTargetType { get; set; }

    public int TargetId { get; set; }

    public long OpenDatetime { get; set; }

    public long CloseDatetime { get; set; }

    public long LatestVersion { get; set; }
}
