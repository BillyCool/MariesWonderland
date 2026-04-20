using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMShopItemCellTable : TableBase<EntityMShopItemCell>
{
    private readonly Func<EntityMShopItemCell, (int, int)> primaryIndexSelector;

    public EntityMShopItemCellTable(EntityMShopItemCell[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.ShopItemCellId, element.StepNumber);
    }

    public RangeView<EntityMShopItemCell> FindRangeByShopItemCellIdAndStepNumber(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
