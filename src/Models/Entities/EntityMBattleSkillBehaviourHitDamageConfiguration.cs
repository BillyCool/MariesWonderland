using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleSkillBehaviourHitDamageConfiguration
{
    public SkillCategoryType SkillCategoryType { get; set; }

    public int HitCount { get; set; }

    public int HitIndexLowerLimit { get; set; }

    public int DamageCoefficientValuePermil { get; set; }
}
