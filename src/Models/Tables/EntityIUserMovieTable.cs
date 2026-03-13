using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserMovieTable : TableBase<EntityIUserMovie>
{
    private readonly Func<EntityIUserMovie, (long, int)> primaryIndexSelector;

    public EntityIUserMovieTable(EntityIUserMovie[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.MovieId);
    }

    public bool TryFindByUserIdAndMovieId(ValueTuple<long, int> key, out EntityIUserMovie result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
