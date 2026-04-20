using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMOmikuji))]
public class EntityMOmikuji
{
    [Key(0)] public int OmikujiId { get; set; }

    [Key(1)] public long StartDatetime { get; set; }

    [Key(2)] public long EndDatetime { get; set; }

    [Key(3)] public int OmikujiAssetId { get; set; }
}
