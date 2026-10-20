using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpGradeOneMatchRewardGroup))]
[PrimaryKey(nameof(PvpGradeOneMatchRewardGroupId), nameof(PvpGradeOneMatchRewardId))]
public class EntityMPvpGradeOneMatchRewardGroup
{
    [Key(0)] public int PvpGradeOneMatchRewardGroupId { get; set; }

    [Key(1)] public int PvpGradeOneMatchRewardId { get; set; }

    [Key(2)] public int Weight { get; set; }
}
