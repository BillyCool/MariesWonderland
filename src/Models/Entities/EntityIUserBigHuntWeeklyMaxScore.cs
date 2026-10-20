using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(BigHuntWeeklyVersion), nameof(AttributeType))]
public class EntityIUserBigHuntWeeklyMaxScore : IUserEntity
{
    public long UserId { get; set; }

    public long BigHuntWeeklyVersion { get; set; }

    public AttributeType AttributeType { get; set; }

    public long MaxScore { get; set; }

    public long LatestVersion { get; set; }
}
