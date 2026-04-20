using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeLevelBonusTable : TableBase<EntityMCostumeLevelBonus>
{
    private readonly Func<EntityMCostumeLevelBonus, (int, int)> primaryIndexSelector;

    public EntityMCostumeLevelBonusTable(EntityMCostumeLevelBonus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CostumeLevelBonusId, element.Level);
    }

    public bool TryFindByCostumeLevelBonusIdAndLevel(ValueTuple<int, int> key, out EntityMCostumeLevelBonus result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, out result);

    public RangeView<EntityMCostumeLevelBonus> FindRangeByCostumeLevelBonusIdAndLevel(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
