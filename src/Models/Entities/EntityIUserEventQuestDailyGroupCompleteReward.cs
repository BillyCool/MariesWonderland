using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserEventQuestDailyGroupCompleteReward : IUserEntity
{
    public long UserId { get; set; }

    public int LastRewardReceiveEventQuestDailyGroupId { get; set; }

    public long LastRewardReceiveDatetime { get; set; }

    public long LatestVersion { get; set; }
}
