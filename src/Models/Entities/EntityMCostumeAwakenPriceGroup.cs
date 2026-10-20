using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeAwakenPriceGroup))]
[PrimaryKey(nameof(CostumeAwakenPriceGroupId), nameof(AwakenStepLowerLimit))]
public class EntityMCostumeAwakenPriceGroup
{
    [Key(0)] public int CostumeAwakenPriceGroupId { get; set; }

    [Key(1)] public int AwakenStepLowerLimit { get; set; }

    [Key(2)] public int Gold { get; set; }
}
