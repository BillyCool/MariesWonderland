using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMShopItemUserLevelConditionTable : TableBase<EntityMShopItemUserLevelCondition>
{
    private readonly Func<EntityMShopItemUserLevelCondition, int> primaryIndexSelector;

    public EntityMShopItemUserLevelConditionTable(EntityMShopItemUserLevelCondition[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ShopItemId;
    }
}
