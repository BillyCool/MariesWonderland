using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCatalogTermTable : TableBase<EntityMCatalogTerm>
{
    private readonly Func<EntityMCatalogTerm, int> primaryIndexSelector;

    public EntityMCatalogTermTable(EntityMCatalogTerm[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CatalogTermId;
    }

    public EntityMCatalogTerm FindByCatalogTermId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
