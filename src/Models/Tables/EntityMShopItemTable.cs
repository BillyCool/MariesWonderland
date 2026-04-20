using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMShopItemTable : TableBase<EntityMShopItem>
{
    private readonly Func<EntityMShopItem, int> primaryIndexSelector;

    public EntityMShopItemTable(EntityMShopItem[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ShopItemId;
    }

    public EntityMShopItem FindByShopItemId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
