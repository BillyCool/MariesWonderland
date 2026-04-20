using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillDamageMultiplyDetailBuffAttached))]
public class EntityMSkillDamageMultiplyDetailBuffAttached
{
    [Key(0)] public int SkillDamageMultiplyDetailId { get; set; }

    [Key(1)] public DamageMultiplyBuffAttachedConditionTargetType BuffAttachedTargetType { get; set; }

    [Key(2)] public DamageMultiplyBuffAttachedTargetBuffType TargetBuffType { get; set; }

    [Key(3)] public DamageMultiplyBuffAttachedTargetStatusKindType TargetStatusKindType { get; set; }

    [Key(4)] public int DamageMultiplyCoefficientValuePermil { get; set; }
}
