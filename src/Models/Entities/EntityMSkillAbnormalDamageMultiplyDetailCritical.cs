using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormalDamageMultiplyDetailCritical
{
    public int DamageMultiplyAbnormalDetailId { get; set; }

    public bool IsCritical { get; set; }

    public int DamageMultiplyCoefficientValuePermil { get; set; }
}
