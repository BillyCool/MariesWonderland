using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMPossessionAcquisitionRouteTable : TableBase<EntityMPossessionAcquisitionRoute>
{
    private readonly Func<EntityMPossessionAcquisitionRoute, (PossessionType, int, int)> primaryIndexSelector;

    public EntityMPossessionAcquisitionRouteTable(EntityMPossessionAcquisitionRoute[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PossessionType, element.PossessionId, element.SortOrder);
    }

    public RangeView<EntityMPossessionAcquisitionRoute> FindRangeByPossessionTypeAndPossessionIdAndSortOrder(ValueTuple<PossessionType, int, int> min, ValueTuple<PossessionType, int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(PossessionType, int, int)>.Default, min, max, ascendant);
}
