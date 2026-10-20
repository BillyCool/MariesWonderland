using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCompanionBaseStatus))]
[PrimaryKey(nameof(CompanionBaseStatusId))]
public class EntityMCompanionBaseStatus
{
    [Key(0)] public int CompanionBaseStatusId { get; set; }

    [Key(1)] public int Attack { get; set; }

    [Key(2)] public int Hp { get; set; }

    [Key(3)] public int Vitality { get; set; }
}
