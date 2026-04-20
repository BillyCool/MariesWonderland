using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMLibraryEventQuestStoryGrouping))]
public class EntityMLibraryEventQuestStoryGrouping
{
    [Key(0)] public int LibraryStoryGroupingId { get; set; }

    [Key(1)] public int EventQuestChapterId { get; set; }

    [Key(2)] public int SortOrder { get; set; }
}
