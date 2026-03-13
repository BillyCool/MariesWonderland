using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCatalogCostumeTable : TableBase<EntityMCatalogCostume>
{
    private readonly Func<EntityMCatalogCostume, int> primaryIndexSelector;

    public EntityMCatalogCostumeTable(EntityMCatalogCostume[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CostumeId;
    }

    public bool TryFindByCostumeId(int key, out EntityMCatalogCostume result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
