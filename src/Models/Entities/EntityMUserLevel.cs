using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMUserLevel))]
[PrimaryKey(nameof(UserLevel))]
public class EntityMUserLevel
{
    [Key(0)] public int UserLevel { get; set; }

    [Key(1)] public int MaxStamina { get; set; }
}
