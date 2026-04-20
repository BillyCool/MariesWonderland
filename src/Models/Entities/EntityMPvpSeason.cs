using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpSeason))]
public class EntityMPvpSeason
{
    [Key(0)] public int PvpSeasonId { get; set; }

    [Key(1)] public string NameAssetPath { get; set; }

    [Key(2)] public long SeasonStartDatetime { get; set; }

    [Key(3)] public long SeasonEndDatetime { get; set; }

    [Key(4)] public int PvpSeasonGroupingId { get; set; }

    [Key(5)] public bool IsInvalid { get; set; }

    [Key(6)] public int PvpWeeklyRankRewardRankGroupId { get; set; }

    [Key(7)] public int PvpSeasonRankRewardRankGroupId { get; set; }

    [Key(8)] public int PvpGradeGroupId { get; set; }

    [Key(9)] public int PvpInitialPointAdditionGroupId { get; set; }

    [Key(10)] public int PvpSeasonDeckPowerThresholdGroupingId { get; set; }
}
