using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMaintenanceTable : TableBase<EntityMMaintenance>
{
    private readonly Func<EntityMMaintenance, int> primaryIndexSelector;

    public EntityMMaintenanceTable(EntityMMaintenance[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MaintenanceId;
    }
}
