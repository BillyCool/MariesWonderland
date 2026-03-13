using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserQuestReplayFlowRewardGroup
{
    public long UserId { get; set; }

    public int QuestReplayFlowRewardGroupId { get; set; }

    public long ReceiveDatetime { get; set; }

    public long LatestVersion { get; set; }
}
