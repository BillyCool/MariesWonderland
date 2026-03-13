using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMLibraryMovie
{
    public int LibraryMovieId { get; set; }

    public int TitleLibraryTextId { get; set; }

    public int LibraryMovieCategoryId { get; set; }

    public int SortOrder { get; set; }

    public int LibraryMovieUnlockConditionId { get; set; }

    public int LibraryMovieUnlockEvaluateConditionId { get; set; }

    public int MovieId { get; set; }
}
