using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntBossGradeGroup))]
[PrimaryKey(nameof(BigHuntBossGradeGroupId), nameof(NecessaryScore))]
public class EntityMBigHuntBossGradeGroup
{
    [Key(0)] public int BigHuntBossGradeGroupId { get; set; }

    [Key(1)] public long NecessaryScore { get; set; }

    [Key(2)] public int AssetGradeIconId { get; set; }
}
