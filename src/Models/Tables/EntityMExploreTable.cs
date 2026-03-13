using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMExploreTable : TableBase<EntityMExplore>
{
    private readonly Func<EntityMExplore, int> primaryIndexSelector;

    public EntityMExploreTable(EntityMExplore[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ExploreId;
    }

    public EntityMExplore FindByExploreId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public RangeView<EntityMExplore> FindRangeByExploreId(int min, int max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<int>.Default, min, max, ascendant);
}
