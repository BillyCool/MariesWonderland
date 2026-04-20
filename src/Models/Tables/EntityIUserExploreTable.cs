using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserExploreTable : TableBase<EntityIUserExplore>
{
    private readonly Func<EntityIUserExplore, long> primaryIndexSelector;

    public EntityIUserExploreTable(EntityIUserExplore[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserExplore FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);

    public bool TryFindByUserId(long key, out EntityIUserExplore result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key, out result);
}
