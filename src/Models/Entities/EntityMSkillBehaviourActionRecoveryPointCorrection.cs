using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionRecoveryPointCorrection))]
public class EntityMSkillBehaviourActionRecoveryPointCorrection
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public RecoveryPointCorrectionTargetSkillType RecoveryPointCorrectionTargetSkillType { get; set; }

    [Key(2)] public int RecoveryPointCorrectionCoefficientValue { get; set; }
}
