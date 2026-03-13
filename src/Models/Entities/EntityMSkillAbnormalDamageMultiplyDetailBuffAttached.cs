using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalDamageMultiplyDetailBuffAttached
{
    public int DamageMultiplyAbnormalDetailId { get; set; }

    public int BuffAttachedTargetType { get; set; }

    public int TargetBuffType { get; set; }

    public int TargetStatusKindType { get; set; }

    public int DamageMultiplyCoefficientValuePermil { get; set; }
}
