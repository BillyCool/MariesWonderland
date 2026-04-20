using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillDamageMultiplyHitIndexValueGroup))]
public class EntityMSkillDamageMultiplyHitIndexValueGroup
{
    [Key(0)] public int SkillDamageMultiplyHitIndexValueGroupId { get; set; }

    [Key(1)] public int SkillDamageMultiplyHitIndexValueGroupIndex { get; set; }

    [Key(2)] public int TotalHitCountConditionLower { get; set; }

    [Key(3)] public int TotalHitCountConditionUpper { get; set; }

    [Key(4)] public int HitIndex { get; set; }

    [Key(5)] public int DamageMultiplyCoefficientValuePermil { get; set; }
}
