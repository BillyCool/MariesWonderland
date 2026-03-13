using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPvpWeeklyRankRewardRankGroupTable : TableBase<EntityMPvpWeeklyRankRewardRankGroup>
{
    private readonly Func<EntityMPvpWeeklyRankRewardRankGroup, (int, int)> primaryIndexSelector;

    public EntityMPvpWeeklyRankRewardRankGroupTable(EntityMPvpWeeklyRankRewardRankGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PvpWeeklyRankRewardRankGroupId, element.RankLowerLimit);
    }

    public RangeView<EntityMPvpWeeklyRankRewardRankGroup> FindRangeByPvpWeeklyRankRewardRankGroupIdAndRankLowerLimit(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
