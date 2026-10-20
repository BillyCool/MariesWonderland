using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMaintenance))]
[PrimaryKey(nameof(MaintenanceId))]
public class EntityMMaintenance
{
    [Key(0)] public int MaintenanceId { get; set; }

    [Key(1)] public long StartDatetime { get; set; }

    [Key(2)] public long EndDatetime { get; set; }

    [Key(3)] public int MaintenanceGroupId { get; set; }
}
