using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(SideStoryQuestId))]
public class EntityIUserSideStoryQuest : IUserEntity
{
    public long UserId { get; set; }

    public int SideStoryQuestId { get; set; }

    public int HeadSideStoryQuestSceneId { get; set; }

    public int SideStoryQuestStateType { get; set; }

    public long LatestVersion { get; set; }
}
