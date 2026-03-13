using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActivationConditionHpRatio
{
    public int SkillBehaviourActivationConditionId { get; set; }

    public SkillBehaviourActivationConditionHpRatioThresholdType SkillBehaviourActivationConditionHpRatioThresholdType { get; set; }

    public int ThresholdRatioPermil { get; set; }
}
