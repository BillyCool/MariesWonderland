using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(QuestId))]
public class EntityIUserQuestLimitContentStatus : IUserEntity
{
    public long UserId { get; set; }

    public int QuestId { get; set; }

    public int LimitContentQuestStatusType { get; set; }

    public int EventQuestChapterId { get; set; }

    public long LatestVersion { get; set; }
}
