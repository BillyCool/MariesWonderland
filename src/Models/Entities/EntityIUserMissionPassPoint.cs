using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(MissionPassId))]
public class EntityIUserMissionPassPoint : IUserEntity
{
    public long UserId { get; set; }

    public int MissionPassId { get; set; }

    public int Point { get; set; }

    public int PremiumRewardReceivedLevel { get; set; }

    public int NoPremiumRewardReceivedLevel { get; set; }

    public long LatestVersion { get; set; }
}
