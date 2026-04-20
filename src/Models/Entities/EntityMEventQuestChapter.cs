using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestChapter))]
public class EntityMEventQuestChapter
{
    [Key(0)] public int EventQuestChapterId { get; set; }

    [Key(1)] public EventQuestType EventQuestType { get; set; }

    [Key(2)] public int SortOrder { get; set; }

    [Key(3)] public int NameEventQuestTextId { get; set; }

    [Key(4)] public int BannerAssetId { get; set; }

    [Key(5)] public int EventQuestLinkId { get; set; }

    [Key(6)] public int EventQuestDisplayItemGroupId { get; set; }

    [Key(7)] public int EventQuestSequenceGroupId { get; set; }

    [Key(8)] public long StartDatetime { get; set; }

    [Key(9)] public long EndDatetime { get; set; }

    [Key(10)] public int DisplaySortOrder { get; set; }
}
