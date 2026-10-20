using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntScoreRewardGroup))]
[PrimaryKey(nameof(BigHuntScoreRewardGroupId), nameof(NecessaryScore))]
public class EntityMBigHuntScoreRewardGroup
{
    [Key(0)] public int BigHuntScoreRewardGroupId { get; set; }

    [Key(1)] public long NecessaryScore { get; set; }

    [Key(2)] public int BigHuntRewardGroupId { get; set; }
}
