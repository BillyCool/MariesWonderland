using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestScenePictureBookReplace))]
public class EntityMQuestScenePictureBookReplace
{
    [Key(0)] public int QuestSceneId { get; set; }

    [Key(1)] public int PictureBookNameQuestTextId { get; set; }

    [Key(2)] public bool IsExcludeSubflow { get; set; }

    [Key(3)] public bool IsExcludeRecollection { get; set; }
}
