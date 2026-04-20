using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserSideStoryQuestSceneProgressStatus : IUserEntity
{
    public long UserId { get; set; }

    public int CurrentSideStoryQuestId { get; set; }

    public int CurrentSideStoryQuestSceneId { get; set; }

    public long LatestVersion { get; set; }
}
