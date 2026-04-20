using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBigHuntScheduleTable : TableBase<EntityMBigHuntSchedule>
{
    private readonly Func<EntityMBigHuntSchedule, int> primaryIndexSelector;

    public EntityMBigHuntScheduleTable(EntityMBigHuntSchedule[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BigHuntScheduleId;
    }

    public EntityMBigHuntSchedule FindByBigHuntScheduleId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
