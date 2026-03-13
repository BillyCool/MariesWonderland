using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMLibraryStoryGroup
{
    public int QuestId { get; set; }

    public int SortOrder { get; set; }

    public int StartQuestSceneId { get; set; }

    public int EndQuestSceneId { get; set; }
}
