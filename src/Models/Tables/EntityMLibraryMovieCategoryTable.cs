using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMLibraryMovieCategoryTable : TableBase<EntityMLibraryMovieCategory>
{
    private readonly Func<EntityMLibraryMovieCategory, int> primaryIndexSelector;

    public EntityMLibraryMovieCategoryTable(EntityMLibraryMovieCategory[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.LibraryMovieCategoryId;
    }
}
