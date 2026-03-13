using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPvpSeasonRankRewardPerSeason
{
    public int RankLowerLimit { get; set; }

    public int PvpSeasonId { get; set; }

    public int PvpSeasonRankRewardGroupId { get; set; }
}
