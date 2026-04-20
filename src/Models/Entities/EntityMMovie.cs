using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMovie))]
public class EntityMMovie
{
    [Key(0)] public int MovieId { get; set; }

    [Key(1)] public int AssetId { get; set; }
}
