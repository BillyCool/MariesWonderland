using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMaintenanceGroup
{
    public int MaintenanceGroupId { get; set; }

    public string ApiPath { get; set; }

    public int Priority { get; set; }

    public ScreenTransitionType ScreenTransitionType { get; set; }

    public MaintenanceBlockFunctionType BlockFunctionType { get; set; }

    public string BlockFunctionValue { get; set; }
}
