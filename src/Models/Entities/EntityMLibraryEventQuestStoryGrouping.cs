using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMLibraryEventQuestStoryGrouping
{
    public int LibraryStoryGroupingId { get; set; }

    public int EventQuestChapterId { get; set; }

    public int SortOrder { get; set; }
}
