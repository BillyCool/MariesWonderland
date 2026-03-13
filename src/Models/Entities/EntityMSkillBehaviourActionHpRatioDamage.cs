using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionHpRatioDamage
{
    public int SkillBehaviourActionId { get; set; }

    public int CalculateDenominatorType { get; set; }

    public int DamageRatioPermil { get; set; }
}
