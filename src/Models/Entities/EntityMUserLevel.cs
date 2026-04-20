using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMUserLevel))]
public class EntityMUserLevel
{
    [Key(0)] public int UserLevel { get; set; }

    [Key(1)] public int MaxStamina { get; set; }
}
