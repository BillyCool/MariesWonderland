using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPvpSeason
{
    public int PvpSeasonId { get; set; }

    public string NameAssetPath { get; set; }

    public long SeasonStartDatetime { get; set; }

    public long SeasonEndDatetime { get; set; }

    public int PvpSeasonGroupingId { get; set; }

    public bool IsInvalid { get; set; }

    public int PvpWeeklyRankRewardRankGroupId { get; set; }

    public int PvpSeasonRankRewardRankGroupId { get; set; }

    public int PvpGradeGroupId { get; set; }

    public int PvpInitialPointAdditionGroupId { get; set; }

    public int PvpSeasonDeckPowerThresholdGroupingId { get; set; }
}
