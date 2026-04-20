using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMLibraryStoryGroup))]
public class EntityMLibraryStoryGroup
{
    [Key(0)] public int QuestId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int StartQuestSceneId { get; set; }

    [Key(3)] public int EndQuestSceneId { get; set; }
}
