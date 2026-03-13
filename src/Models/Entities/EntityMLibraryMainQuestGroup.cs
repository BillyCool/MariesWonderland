using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMLibraryMainQuestGroup
{
    public int LibraryMainQuestGroupId { get; set; }

    public int MainQuestChapterId { get; set; }

    public int SortOrder { get; set; }

    public int ChapterTextAssetId { get; set; }

    public int FirstStillAssetOrder { get; set; }

    public int SecondStillAssetOrder { get; set; }
}
