using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityIUserBigHuntWeeklyMaxScoreTable : TableBase<EntityIUserBigHuntWeeklyMaxScore>
{
    private readonly Func<EntityIUserBigHuntWeeklyMaxScore, (long, long, AttributeType)> primaryIndexSelector;

    public EntityIUserBigHuntWeeklyMaxScoreTable(EntityIUserBigHuntWeeklyMaxScore[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.BigHuntWeeklyVersion, element.AttributeType);
    }

    public bool TryFindByUserIdAndBigHuntWeeklyVersionAndAttributeType(ValueTuple<long, long, AttributeType> key, out EntityIUserBigHuntWeeklyMaxScore result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, long, AttributeType)>.Default, key, out result);
}
