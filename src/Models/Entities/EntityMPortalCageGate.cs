using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPortalCageGate))]
public class EntityMPortalCageGate
{
    [Key(0)] public int PortalCageGateId { get; set; }

    [Key(1)] public int GatePositionIndex { get; set; }

    [Key(2)] public int PortalCageAccessPointFunctionGroupScheduleId { get; set; }
}
