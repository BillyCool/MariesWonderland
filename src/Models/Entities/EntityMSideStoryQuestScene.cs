using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSideStoryQuestScene))]
public class EntityMSideStoryQuestScene
{
    [Key(0)] public int SideStoryQuestId { get; set; }

    [Key(1)] public int SideStoryQuestSceneId { get; set; }

    [Key(2)] public int SortOrder { get; set; }

    [Key(3)] public int AssetBackgroundId { get; set; }

    [Key(4)] public int EventMapNumberUpper { get; set; }

    [Key(5)] public int EventMapNumberLower { get; set; }
}
