using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeAwakenEffectGroup))]
[PrimaryKey(nameof(CostumeAwakenEffectGroupId), nameof(AwakenStep), nameof(CostumeAwakenEffectType))]
public class EntityMCostumeAwakenEffectGroup
{
    [Key(0)] public int CostumeAwakenEffectGroupId { get; set; }

    [Key(1)] public int AwakenStep { get; set; }

    [Key(2)] public CostumeAwakenEffectType CostumeAwakenEffectType { get; set; }

    [Key(3)] public int CostumeAwakenEffectId { get; set; }
}
