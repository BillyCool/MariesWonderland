using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserMissionTable : TableBase<EntityIUserMission>
{
    private readonly Func<EntityIUserMission, (long, int)> primaryIndexSelector;

    public EntityIUserMissionTable(EntityIUserMission[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.MissionId);
    }

    public EntityIUserMission FindByUserIdAndMissionId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);

    public bool TryFindByUserIdAndMissionId(ValueTuple<long, int> key, out EntityIUserMission result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
