using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpSeasonRankRewardPerSeason))]
[PrimaryKey(nameof(RankLowerLimit), nameof(PvpSeasonId))]
public class EntityMPvpSeasonRankRewardPerSeason
{
    [Key(0)] public int RankLowerLimit { get; set; }

    [Key(1)] public int PvpSeasonId { get; set; }

    [Key(2)] public int PvpSeasonRankRewardGroupId { get; set; }
}
