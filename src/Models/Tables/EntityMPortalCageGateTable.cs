using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPortalCageGateTable : TableBase<EntityMPortalCageGate>
{
    private readonly Func<EntityMPortalCageGate, (int, int)> primaryIndexSelector;

    public EntityMPortalCageGateTable(EntityMPortalCageGate[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PortalCageGateId, element.GatePositionIndex);
    }

    public EntityMPortalCageGate FindByPortalCageGateIdAndGatePositionIndex(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);
}
