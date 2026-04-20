using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPortalCageAccessPointFunctionGroupSchedule))]
public class EntityMPortalCageAccessPointFunctionGroupSchedule
{
    [Key(0)] public int PortalCageAccessPointFunctionGroupScheduleId { get; set; }

    [Key(1)] public int PriorityDesc { get; set; }

    [Key(2)] public int AccessPointType { get; set; }

    [Key(3)] public int AccessPointFunctionGroupId { get; set; }

    [Key(4)] public long StartDatetime { get; set; }

    [Key(5)] public long EndDatetime { get; set; }
}
