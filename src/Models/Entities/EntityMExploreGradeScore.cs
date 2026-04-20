using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMExploreGradeScore))]
public class EntityMExploreGradeScore
{
    [Key(0)] public int ExploreId { get; set; }

    [Key(1)] public int NecessaryScore { get; set; }

    [Key(2)] public int ExploreGradeId { get; set; }
}
