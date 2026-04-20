using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeAwakenStepMaterialGroup))]
public class EntityMCostumeAwakenStepMaterialGroup
{
    [Key(0)] public int CostumeAwakenStepMaterialGroupId { get; set; }

    [Key(1)] public int AwakenStepLowerLimit { get; set; }

    [Key(2)] public int CostumeAwakenMaterialGroupId { get; set; }
}
