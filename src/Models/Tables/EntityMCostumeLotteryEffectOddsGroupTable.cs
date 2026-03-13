using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeLotteryEffectOddsGroupTable : TableBase<EntityMCostumeLotteryEffectOddsGroup>
{
    private readonly Func<EntityMCostumeLotteryEffectOddsGroup, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMCostumeLotteryEffectOddsGroup, int> secondaryIndexSelector;

    public EntityMCostumeLotteryEffectOddsGroupTable(EntityMCostumeLotteryEffectOddsGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CostumeLotteryEffectOddsGroupId, element.OddsNumber);
        secondaryIndexSelector = element => element.CostumeLotteryEffectOddsGroupId;
    }

    public bool TryFindByCostumeLotteryEffectOddsGroupIdAndOddsNumber(ValueTuple<int, int> key, out EntityMCostumeLotteryEffectOddsGroup result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, out result);

    public RangeView<EntityMCostumeLotteryEffectOddsGroup> FindByCostumeLotteryEffectOddsGroupId(int key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
