using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillDamageMultiplyHitIndexValueGroup
{
    public int SkillDamageMultiplyHitIndexValueGroupId { get; set; }

    public int SkillDamageMultiplyHitIndexValueGroupIndex { get; set; }

    public int TotalHitCountConditionLower { get; set; }

    public int TotalHitCountConditionUpper { get; set; }

    public int HitIndex { get; set; }

    public int DamageMultiplyCoefficientValuePermil { get; set; }
}
