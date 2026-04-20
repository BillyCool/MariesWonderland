using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserBigHuntScheduleMaxScoreTable : TableBase<EntityIUserBigHuntScheduleMaxScore>
{
    private readonly Func<EntityIUserBigHuntScheduleMaxScore, (long, int, int)> primaryIndexSelector;

    public EntityIUserBigHuntScheduleMaxScoreTable(EntityIUserBigHuntScheduleMaxScore[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.BigHuntScheduleId, element.BigHuntBossId);
    }

    public EntityIUserBigHuntScheduleMaxScore FindByUserIdAndBigHuntScheduleIdAndBigHuntBossId(ValueTuple<long, int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int, int)>.Default, key);

    public bool TryFindByUserIdAndBigHuntScheduleIdAndBigHuntBossId(ValueTuple<long, int, int> key, out EntityIUserBigHuntScheduleMaxScore result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int, int)>.Default, key, out result);
}
