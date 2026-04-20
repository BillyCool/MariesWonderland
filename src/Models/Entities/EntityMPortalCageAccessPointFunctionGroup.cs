using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPortalCageAccessPointFunctionGroup))]
public class EntityMPortalCageAccessPointFunctionGroup
{
    [Key(0)] public int AccessPointFunctionGroupId { get; set; }

    [Key(1)] public int AccessPointFunctionIndex { get; set; }

    [Key(2)] public int AccessPointFunctionId { get; set; }
}
