using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpSeasonRankReward))]
public class EntityMPvpSeasonRankReward
{
    [Key(0)] public int RankLowerLimit { get; set; }

    [Key(1)] public int PvpSeasonRankRewardGroupId { get; set; }
}
