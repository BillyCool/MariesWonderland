using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserEventQuestLabyrinthStage
{
    public long UserId { get; set; }

    public int EventQuestChapterId { get; set; }

    public int StageOrder { get; set; }

    public bool IsReceivedStageClearReward { get; set; }

    public int AccumulationRewardReceivedQuestMissionCount { get; set; }

    public long LatestVersion { get; set; }
}
