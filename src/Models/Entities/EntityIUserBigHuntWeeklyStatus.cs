using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserBigHuntWeeklyStatus : IUserEntity
{
    public long UserId { get; set; }

    public long BigHuntWeeklyVersion { get; set; }

    public bool IsReceivedWeeklyReward { get; set; }

    public long LatestVersion { get; set; }
}
