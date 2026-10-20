using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpGradeWeeklyRewardGroup))]
[PrimaryKey(nameof(PvpGradeWeeklyRewardGroupId), nameof(PvpRewardId))]
public class EntityMPvpGradeWeeklyRewardGroup
{
    [Key(0)] public int PvpGradeWeeklyRewardGroupId { get; set; }

    [Key(1)] public int PvpRewardId { get; set; }

    [Key(2)] public int SortOrder { get; set; }
}
