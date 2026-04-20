using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMainQuestChapter))]
public class EntityMMainQuestChapter
{
    [Key(0)] public int MainQuestChapterId { get; set; }

    [Key(1)] public int MainQuestRouteId { get; set; }

    [Key(2)] public int SortOrder { get; set; }

    [Key(3)] public int MainQuestSequenceGroupId { get; set; }

    [Key(4)] public int PortalCageCharacterGroupId { get; set; }

    [Key(5)] public long StartDatetime { get; set; }

    [Key(6)] public bool IsInvisibleInLibrary { get; set; }

    [Key(7)] public int JoinLibraryChapterId { get; set; }
}
