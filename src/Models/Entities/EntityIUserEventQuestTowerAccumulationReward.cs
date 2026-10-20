using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(EventQuestChapterId))]
public class EntityIUserEventQuestTowerAccumulationReward : IUserEntity
{
    public long UserId { get; set; }

    public int EventQuestChapterId { get; set; }

    public int LatestRewardReceiveQuestMissionClearCount { get; set; }

    public long LatestVersion { get; set; }
}
