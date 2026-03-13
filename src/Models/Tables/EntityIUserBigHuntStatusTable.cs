using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserBigHuntStatusTable : TableBase<EntityIUserBigHuntStatus>
{
    private readonly Func<EntityIUserBigHuntStatus, (long, int)> primaryIndexSelector;

    public EntityIUserBigHuntStatusTable(EntityIUserBigHuntStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.BigHuntBossQuestId);
    }

    public EntityIUserBigHuntStatus FindByUserIdAndBigHuntBossQuestId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);
}
