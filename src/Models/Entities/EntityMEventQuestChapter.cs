using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestChapter
{
    public int EventQuestChapterId { get; set; }

    public EventQuestType EventQuestType { get; set; }

    public int SortOrder { get; set; }

    public int NameEventQuestTextId { get; set; }

    public int BannerAssetId { get; set; }

    public int EventQuestLinkId { get; set; }

    public int EventQuestDisplayItemGroupId { get; set; }

    public int EventQuestSequenceGroupId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int DisplaySortOrder { get; set; }
}
