using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMExploreGradeScore))]
[PrimaryKey(nameof(ExploreId), nameof(NecessaryScore))]
public class EntityMExploreGradeScore
{
    [Key(0)] public int ExploreId { get; set; }

    [Key(1)] public int NecessaryScore { get; set; }

    [Key(2)] public int ExploreGradeId { get; set; }
}
