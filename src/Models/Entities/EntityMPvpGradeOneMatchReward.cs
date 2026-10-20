using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpGradeOneMatchReward))]
[PrimaryKey(nameof(PvpGradeOneMatchRewardId), nameof(PvpRewardId))]
public class EntityMPvpGradeOneMatchReward
{
    [Key(0)] public int PvpGradeOneMatchRewardId { get; set; }

    [Key(1)] public int PvpRewardId { get; set; }

    [Key(2)] public int SortOrder { get; set; }
}
