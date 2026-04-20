using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMAssetCalculatorTable : TableBase<EntityMAssetCalculator>
{
    private readonly Func<EntityMAssetCalculator, int> primaryIndexSelector;

    public EntityMAssetCalculatorTable(EntityMAssetCalculator[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.AssetCalculatorId;
    }

    public EntityMAssetCalculator FindByAssetCalculatorId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
