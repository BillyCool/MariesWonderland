using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMGimmickSequenceScheduleTable : TableBase<EntityMGimmickSequenceSchedule>
{
    private readonly Func<EntityMGimmickSequenceSchedule, int> primaryIndexSelector;

    public EntityMGimmickSequenceScheduleTable(EntityMGimmickSequenceSchedule[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.GimmickSequenceScheduleId;
    }
}
