using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMainQuestChapter
{
    public int MainQuestChapterId { get; set; }

    public int MainQuestRouteId { get; set; }

    public int SortOrder { get; set; }

    public int MainQuestSequenceGroupId { get; set; }

    public int PortalCageCharacterGroupId { get; set; }

    public long StartDatetime { get; set; }

    public bool IsInvisibleInLibrary { get; set; }

    public int JoinLibraryChapterId { get; set; }
}
