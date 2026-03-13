using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMShopItemContentPossessionTable : TableBase<EntityMShopItemContentPossession>
{
    private readonly Func<EntityMShopItemContentPossession, (int, PossessionType, int)> primaryIndexSelector;

    public EntityMShopItemContentPossessionTable(EntityMShopItemContentPossession[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.ShopItemId, element.PossessionType, element.PossessionId);
    }
}
