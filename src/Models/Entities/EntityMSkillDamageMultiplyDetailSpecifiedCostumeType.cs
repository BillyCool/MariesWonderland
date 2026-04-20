using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillDamageMultiplyDetailSpecifiedCostumeType))]
public class EntityMSkillDamageMultiplyDetailSpecifiedCostumeType
{
    [Key(0)] public int SkillDamageMultiplyDetailId { get; set; }

    [Key(1)] public DamageMultiplySpecifiedCostumeConditionTargetType SpecifiedCostumeConditionTargetType { get; set; }

    [Key(2)] public int TargetSpecifiedCostumeGroupId { get; set; }

    [Key(3)] public int DamageMultiplyCoefficientValuePermil { get; set; }
}
