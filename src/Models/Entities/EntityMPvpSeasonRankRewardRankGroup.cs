using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpSeasonRankRewardRankGroup))]
public class EntityMPvpSeasonRankRewardRankGroup
{
    [Key(0)] public int PvpSeasonRankRewardRankGroupId { get; set; }

    [Key(1)] public int RankLowerLimit { get; set; }

    [Key(2)] public int PvpSeasonRankRewardGroupId { get; set; }
}
