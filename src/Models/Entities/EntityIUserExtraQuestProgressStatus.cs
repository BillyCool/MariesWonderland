using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserExtraQuestProgressStatus : IUserEntity
{
    public long UserId { get; set; }

    public int CurrentQuestId { get; set; }

    public int CurrentQuestSceneId { get; set; }

    public int HeadQuestSceneId { get; set; }

    public long LatestVersion { get; set; }
}
