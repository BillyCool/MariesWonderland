using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionDamageCorrectionHpRatio
{
    public int SkillBehaviourActionId { get; set; }

    public int CorrectionMaxValuePermil { get; set; }

    public DamageCorrectionHpRatioType DamageCorrectionHpRatioType { get; set; }

    public int ActivationThresholdHpRatioPermil { get; set; }
}
