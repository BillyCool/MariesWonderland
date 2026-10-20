using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(BigHuntWeeklyVersion))]
public class EntityIUserBigHuntWeeklyStatus : IUserEntity
{
    public long UserId { get; set; }

    public long BigHuntWeeklyVersion { get; set; }

    public bool IsReceivedWeeklyReward { get; set; }

    public long LatestVersion { get; set; }
}
