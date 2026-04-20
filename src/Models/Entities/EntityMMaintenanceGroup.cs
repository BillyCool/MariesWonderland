using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMaintenanceGroup))]
public class EntityMMaintenanceGroup
{
    [Key(0)] public int MaintenanceGroupId { get; set; }

    [Key(1)] public string ApiPath { get; set; }

    [Key(2)] public int Priority { get; set; }

    [Key(3)] public ScreenTransitionType ScreenTransitionType { get; set; }

    [Key(4)] public MaintenanceBlockFunctionType BlockFunctionType { get; set; }

    [Key(5)] public string BlockFunctionValue { get; set; }
}
