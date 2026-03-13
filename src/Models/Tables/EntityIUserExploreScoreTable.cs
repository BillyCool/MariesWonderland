using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserExploreScoreTable : TableBase<EntityIUserExploreScore>
{
    private readonly Func<EntityIUserExploreScore, (long, int)> primaryIndexSelector;

    public EntityIUserExploreScoreTable(EntityIUserExploreScore[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.ExploreId);
    }

    public EntityIUserExploreScore FindByUserIdAndExploreId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);
}
