using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMShopItemCellLimitedOpenTable : TableBase<EntityMShopItemCellLimitedOpen>
{
    private readonly Func<EntityMShopItemCellLimitedOpen, int> primaryIndexSelector;

    public EntityMShopItemCellLimitedOpenTable(EntityMShopItemCellLimitedOpen[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ShopItemCellId;
    }

    public EntityMShopItemCellLimitedOpen FindByShopItemCellId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
