using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviour
{
    public int SkillBehaviourId { get; set; }

    public SkillBehaviourType SkillBehaviourType { get; set; }

    public int SkillBehaviourActionId { get; set; }

    public int SkillBehaviourActivationMethodId { get; set; }

    public int SkillBehaviourAssetCalculatorId { get; set; }

    public int HitRatioPermil { get; set; }

    public SkillBehaviourLifetimeCalculationMethodType SkillBehaviourLifetimeCalculationMethodType { get; set; }

    public int LifetimeCount { get; set; }

    public int SkillTargetScopeAssetCalculatorId { get; set; }
}
