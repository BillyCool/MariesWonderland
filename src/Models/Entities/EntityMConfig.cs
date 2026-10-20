using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMConfig))]
[PrimaryKey(nameof(ConfigKey))]
public class EntityMConfig
{
    [Key(0)] public string ConfigKey { get; set; }

    [Key(1)] public string Value { get; set; }
}
