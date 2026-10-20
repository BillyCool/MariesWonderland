using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMovie))]
[PrimaryKey(nameof(MovieId))]
public class EntityMMovie
{
    [Key(0)] public int MovieId { get; set; }

    [Key(1)] public int AssetId { get; set; }
}
