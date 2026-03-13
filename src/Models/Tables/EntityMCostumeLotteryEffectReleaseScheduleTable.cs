using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeLotteryEffectReleaseScheduleTable : TableBase<EntityMCostumeLotteryEffectReleaseSchedule>
{
    private readonly Func<EntityMCostumeLotteryEffectReleaseSchedule, int> primaryIndexSelector;

    public EntityMCostumeLotteryEffectReleaseScheduleTable(EntityMCostumeLotteryEffectReleaseSchedule[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CostumeLotteryEffectReleaseScheduleId;
    }

    public bool TryFindByCostumeLotteryEffectReleaseScheduleId(int key, out EntityMCostumeLotteryEffectReleaseSchedule result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
