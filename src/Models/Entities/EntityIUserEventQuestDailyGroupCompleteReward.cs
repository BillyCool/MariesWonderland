using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserEventQuestDailyGroupCompleteReward : IUserEntity
{
    public long UserId { get; set; }

    public int LastRewardReceiveEventQuestDailyGroupId { get; set; }

    public long LastRewardReceiveDatetime { get; set; }

    public long LatestVersion { get; set; }
}
