using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSideStoryQuestScene
{
    public int SideStoryQuestId { get; set; }

    public int SideStoryQuestSceneId { get; set; }

    public int SortOrder { get; set; }

    public int AssetBackgroundId { get; set; }

    public int EventMapNumberUpper { get; set; }

    public int EventMapNumberLower { get; set; }
}
