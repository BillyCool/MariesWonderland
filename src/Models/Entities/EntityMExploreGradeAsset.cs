using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMExploreGradeAsset))]
public class EntityMExploreGradeAsset
{
    [Key(0)] public int ExploreGradeId { get; set; }

    [Key(1)] public int AssetGradeIconId { get; set; }
}
