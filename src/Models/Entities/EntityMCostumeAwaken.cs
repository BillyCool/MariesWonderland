using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeAwaken))]
[PrimaryKey(nameof(CostumeId))]
public class EntityMCostumeAwaken
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public int CostumeAwakenEffectGroupId { get; set; }

    [Key(2)] public int CostumeAwakenStepMaterialGroupId { get; set; }

    [Key(3)] public int CostumeAwakenPriceGroupId { get; set; }
}
