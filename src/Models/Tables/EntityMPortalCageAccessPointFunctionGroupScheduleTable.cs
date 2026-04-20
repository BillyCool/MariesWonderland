using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPortalCageAccessPointFunctionGroupScheduleTable : TableBase<EntityMPortalCageAccessPointFunctionGroupSchedule>
{
    private readonly Func<EntityMPortalCageAccessPointFunctionGroupSchedule, (int, int)> primaryIndexSelector;

    public EntityMPortalCageAccessPointFunctionGroupScheduleTable(EntityMPortalCageAccessPointFunctionGroupSchedule[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PortalCageAccessPointFunctionGroupScheduleId, element.PriorityDesc);
    }
}
