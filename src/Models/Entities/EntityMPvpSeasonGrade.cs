using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpSeasonGrade))]
[PrimaryKey(nameof(PvpGradeId), nameof(PvpSeasonId))]
public class EntityMPvpSeasonGrade
{
    [Key(0)] public int PvpGradeId { get; set; }

    [Key(1)] public int PvpSeasonId { get; set; }

    [Key(2)] public int NecessaryPvpPoint { get; set; }

    [Key(3)] public int IconAssetId { get; set; }

    [Key(4)] public int PvpGradeWeeklyRewardGroupId { get; set; }

    [Key(5)] public int PvpGradeOneMatchRewardGroupId { get; set; }
}
