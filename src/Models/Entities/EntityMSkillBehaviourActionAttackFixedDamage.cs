using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionAttackFixedDamage
{
    public int SkillBehaviourActionId { get; set; }

    public int DamageValue { get; set; }

    public bool ForceDamage { get; set; }
}
