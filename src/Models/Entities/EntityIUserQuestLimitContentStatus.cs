using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserQuestLimitContentStatus : IUserEntity
{
    public long UserId { get; set; }

    public int QuestId { get; set; }

    public int LimitContentQuestStatusType { get; set; }

    public int EventQuestChapterId { get; set; }

    public long LatestVersion { get; set; }
}
