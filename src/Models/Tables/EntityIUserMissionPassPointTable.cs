using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserMissionPassPointTable : TableBase<EntityIUserMissionPassPoint>
{
    private readonly Func<EntityIUserMissionPassPoint, (long, int)> primaryIndexSelector;

    public EntityIUserMissionPassPointTable(EntityIUserMissionPassPoint[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.MissionPassId);
    }

    public EntityIUserMissionPassPoint FindByUserIdAndMissionPassId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);

    public bool TryFindByUserIdAndMissionPassId(ValueTuple<long, int> key, out EntityIUserMissionPassPoint result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
