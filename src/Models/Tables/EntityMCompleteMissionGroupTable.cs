using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMCompleteMissionGroupTable : TableBase<EntityMCompleteMissionGroup>
{
    private readonly Func<EntityMCompleteMissionGroup, (int, PossessionType, int)> primaryIndexSelector;

    public EntityMCompleteMissionGroupTable(EntityMCompleteMissionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.MissionId, element.PossessionType, element.PossessionId);
    }
}
