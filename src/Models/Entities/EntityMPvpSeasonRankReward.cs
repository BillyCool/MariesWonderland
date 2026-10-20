using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpSeasonRankReward))]
[PrimaryKey(nameof(RankLowerLimit))]
public class EntityMPvpSeasonRankReward
{
    [Key(0)] public int RankLowerLimit { get; set; }

    [Key(1)] public int PvpSeasonRankRewardGroupId { get; set; }
}
