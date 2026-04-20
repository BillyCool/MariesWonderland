using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpSeasonRankRewardPerSeason))]
public class EntityMPvpSeasonRankRewardPerSeason
{
    [Key(0)] public int RankLowerLimit { get; set; }

    [Key(1)] public int PvpSeasonId { get; set; }

    [Key(2)] public int PvpSeasonRankRewardGroupId { get; set; }
}
