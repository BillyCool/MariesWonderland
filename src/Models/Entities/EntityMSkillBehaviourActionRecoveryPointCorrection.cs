using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionRecoveryPointCorrection
{
    public int SkillBehaviourActionId { get; set; }

    public RecoveryPointCorrectionTargetSkillType RecoveryPointCorrectionTargetSkillType { get; set; }

    public int RecoveryPointCorrectionCoefficientValue { get; set; }
}
