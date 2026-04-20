using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCatalogThoughtTable : TableBase<EntityMCatalogThought>
{
    private readonly Func<EntityMCatalogThought, int> primaryIndexSelector;

    public EntityMCatalogThoughtTable(EntityMCatalogThought[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ThoughtId;
    }

    public EntityMCatalogThought FindByThoughtId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
