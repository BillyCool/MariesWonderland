using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestScenePictureBookReplace
{
    public int QuestSceneId { get; set; }

    public int PictureBookNameQuestTextId { get; set; }

    public bool IsExcludeSubflow { get; set; }

    public bool IsExcludeRecollection { get; set; }
}
