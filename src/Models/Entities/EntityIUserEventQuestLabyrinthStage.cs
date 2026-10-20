using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(EventQuestChapterId), nameof(StageOrder))]
public class EntityIUserEventQuestLabyrinthStage : IUserEntity
{
    public long UserId { get; set; }

    public int EventQuestChapterId { get; set; }

    public int StageOrder { get; set; }

    public bool IsReceivedStageClearReward { get; set; }

    public int AccumulationRewardReceivedQuestMissionCount { get; set; }

    public long LatestVersion { get; set; }
}
