using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpSeasonRankRewardRankGroup))]
[PrimaryKey(nameof(PvpSeasonRankRewardRankGroupId), nameof(RankLowerLimit))]
public class EntityMPvpSeasonRankRewardRankGroup
{
    [Key(0)] public int PvpSeasonRankRewardRankGroupId { get; set; }

    [Key(1)] public int RankLowerLimit { get; set; }

    [Key(2)] public int PvpSeasonRankRewardGroupId { get; set; }
}
