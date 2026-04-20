using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroup))]
public class EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroup
{
    [Key(0)] public int TargetSpecifiedCostumeGroupId { get; set; }

    [Key(1)] public int TargetSpecifiedCostumeGroupIndex { get; set; }

    [Key(2)] public int CostumeId { get; set; }
}
