using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPortalCageAccessPointFunctionGroupSchedule
{
    public int PortalCageAccessPointFunctionGroupScheduleId { get; set; }

    public int PriorityDesc { get; set; }

    public int AccessPointType { get; set; }

    public int AccessPointFunctionGroupId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }
}
