using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMExploreGradeAsset))]
[PrimaryKey(nameof(ExploreGradeId))]
public class EntityMExploreGradeAsset
{
    [Key(0)] public int ExploreGradeId { get; set; }

    [Key(1)] public int AssetGradeIconId { get; set; }
}
