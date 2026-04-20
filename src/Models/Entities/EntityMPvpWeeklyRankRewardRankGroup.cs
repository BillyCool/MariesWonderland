using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpWeeklyRankRewardRankGroup))]
public class EntityMPvpWeeklyRankRewardRankGroup
{
    [Key(0)] public int PvpWeeklyRankRewardRankGroupId { get; set; }

    [Key(1)] public int RankLowerLimit { get; set; }

    [Key(2)] public int PvpWeeklyRankRewardGroupId { get; set; }
}
