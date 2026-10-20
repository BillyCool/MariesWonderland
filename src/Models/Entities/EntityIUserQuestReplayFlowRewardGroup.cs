using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(QuestReplayFlowRewardGroupId))]
public class EntityIUserQuestReplayFlowRewardGroup : IUserEntity
{
    public long UserId { get; set; }

    public int QuestReplayFlowRewardGroupId { get; set; }

    public long ReceiveDatetime { get; set; }

    public long LatestVersion { get; set; }
}
