using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPvpSeasonRankRewardPerSeasonTable : TableBase<EntityMPvpSeasonRankRewardPerSeason>
{
    private readonly Func<EntityMPvpSeasonRankRewardPerSeason, (int, int)> primaryIndexSelector;

    public EntityMPvpSeasonRankRewardPerSeasonTable(EntityMPvpSeasonRankRewardPerSeason[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.RankLowerLimit, element.PvpSeasonId);
    }
}
