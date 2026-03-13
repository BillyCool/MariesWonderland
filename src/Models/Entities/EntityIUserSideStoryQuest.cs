using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserSideStoryQuest
{
    public long UserId { get; set; }

    public int SideStoryQuestId { get; set; }

    public int HeadSideStoryQuestSceneId { get; set; }

    public int SideStoryQuestStateType { get; set; }

    public long LatestVersion { get; set; }
}
