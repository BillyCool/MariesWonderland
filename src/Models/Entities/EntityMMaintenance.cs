using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMaintenance
{
    public int MaintenanceId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int MaintenanceGroupId { get; set; }
}
