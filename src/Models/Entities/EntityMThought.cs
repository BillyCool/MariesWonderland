using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMThought))]
public class EntityMThought
{
    [Key(0)] public int ThoughtId { get; set; }

    [Key(1)] public RarityType RarityType { get; set; }

    [Key(2)] public int AbilityId { get; set; }

    [Key(3)] public int AbilityLevel { get; set; }

    [Key(4)] public int ThoughtAssetId { get; set; }
}
