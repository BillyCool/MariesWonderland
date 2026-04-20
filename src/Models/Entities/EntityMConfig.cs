using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMConfig))]
public class EntityMConfig
{
    [Key(0)] public string ConfigKey { get; set; }

    [Key(1)] public string Value { get; set; }
}
