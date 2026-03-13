using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeLotteryEffectTable : TableBase<EntityMCostumeLotteryEffect>
{
    private readonly Func<EntityMCostumeLotteryEffect, (int, int)> primaryIndexSelector;

    public EntityMCostumeLotteryEffectTable(EntityMCostumeLotteryEffect[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CostumeId, element.SlotNumber);
    }

    public bool TryFindByCostumeIdAndSlotNumber(ValueTuple<int, int> key, out EntityMCostumeLotteryEffect result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, out result);

    public RangeView<EntityMCostumeLotteryEffect> FindRangeByCostumeIdAndSlotNumber(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
