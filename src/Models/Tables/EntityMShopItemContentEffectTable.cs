using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMShopItemContentEffectTable : TableBase<EntityMShopItemContentEffect>
{
    private readonly Func<EntityMShopItemContentEffect, int> primaryIndexSelector;

    public EntityMShopItemContentEffectTable(EntityMShopItemContentEffect[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ShopItemId;
    }

    public EntityMShopItemContentEffect FindByShopItemId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
