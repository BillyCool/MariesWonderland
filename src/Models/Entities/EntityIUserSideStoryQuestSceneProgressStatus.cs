using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserSideStoryQuestSceneProgressStatus : IUserEntity
{
    public long UserId { get; set; }

    public int CurrentSideStoryQuestId { get; set; }

    public int CurrentSideStoryQuestSceneId { get; set; }

    public long LatestVersion { get; set; }
}
