using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpSeasonRankRewardGroup))]
public class EntityMPvpSeasonRankRewardGroup
{
    [Key(0)] public int PvpSeasonRankRewardGroupId { get; set; }

    [Key(1)] public int PvpRewardId { get; set; }

    [Key(2)] public int SortOrder { get; set; }
}
