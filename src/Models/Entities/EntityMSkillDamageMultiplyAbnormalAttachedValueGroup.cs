using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillDamageMultiplyAbnormalAttachedValueGroup))]
public class EntityMSkillDamageMultiplyAbnormalAttachedValueGroup
{
    [Key(0)] public int SkillDamageMultiplyAbnormalAttachedValueGroupId { get; set; }

    [Key(1)] public int SkillDamageMultiplyAbnormalAttachedValueGroupIndex { get; set; }

    [Key(2)] public DamageMultiplyAbnormalAttachedPolarityConditionType PolarityConditionType { get; set; }

    [Key(3)] public int SkillAbnormalTypeIdCondition { get; set; }

    [Key(4)] public DamageMultiplyAbnormalAttachedTargetType TargetType { get; set; }

    [Key(5)] public int DamageMultiplyCoefficientValuePermil { get; set; }
}
