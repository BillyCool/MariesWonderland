using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPvpSeasonRankRewardTable : TableBase<EntityMPvpSeasonRankReward>
{
    private readonly Func<EntityMPvpSeasonRankReward, int> primaryIndexSelector;

    public EntityMPvpSeasonRankRewardTable(EntityMPvpSeasonRankReward[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.RankLowerLimit;
    }
}
