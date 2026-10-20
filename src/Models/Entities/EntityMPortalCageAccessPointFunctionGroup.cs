using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPortalCageAccessPointFunctionGroup))]
[PrimaryKey(nameof(AccessPointFunctionGroupId), nameof(AccessPointFunctionIndex))]
public class EntityMPortalCageAccessPointFunctionGroup
{
    [Key(0)] public int AccessPointFunctionGroupId { get; set; }

    [Key(1)] public int AccessPointFunctionIndex { get; set; }

    [Key(2)] public int AccessPointFunctionId { get; set; }
}
