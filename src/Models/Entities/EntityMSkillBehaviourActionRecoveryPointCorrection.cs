using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionRecoveryPointCorrection))]
[PrimaryKey(nameof(SkillBehaviourActionId))]
public class EntityMSkillBehaviourActionRecoveryPointCorrection
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public RecoveryPointCorrectionTargetSkillType RecoveryPointCorrectionTargetSkillType { get; set; }

    [Key(2)] public int RecoveryPointCorrectionCoefficientValue { get; set; }
}
