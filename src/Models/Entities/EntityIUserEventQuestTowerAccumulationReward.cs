using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserEventQuestTowerAccumulationReward : IUserEntity
{
    public long UserId { get; set; }

    public int EventQuestChapterId { get; set; }

    public int LatestRewardReceiveQuestMissionClearCount { get; set; }

    public long LatestVersion { get; set; }
}
