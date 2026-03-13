using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillDamageMultiplyDetailCritical
{
    public int SkillDamageMultiplyDetailId { get; set; }

    public bool IsCritical { get; set; }

    public int DamageMultiplyCoefficientValuePermil { get; set; }
}
