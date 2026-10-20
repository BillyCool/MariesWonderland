using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeAwakenStepMaterialGroup))]
[PrimaryKey(nameof(CostumeAwakenStepMaterialGroupId), nameof(AwakenStepLowerLimit))]
public class EntityMCostumeAwakenStepMaterialGroup
{
    [Key(0)] public int CostumeAwakenStepMaterialGroupId { get; set; }

    [Key(1)] public int AwakenStepLowerLimit { get; set; }

    [Key(2)] public int CostumeAwakenMaterialGroupId { get; set; }
}
