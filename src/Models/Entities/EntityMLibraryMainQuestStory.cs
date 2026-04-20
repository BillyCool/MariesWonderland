using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMLibraryMainQuestStory))]
public class EntityMLibraryMainQuestStory
{
    [Key(0)] public int LibraryMainQuestGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int RecollectionSceneId { get; set; }

    [Key(3)] public int LibraryMainQuestStoryUnlockEvaluateConditionId { get; set; }

    [Key(4)] public int TextAssetId { get; set; }
}
