using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionSkillRecoveryPowerCorrection))]
public class EntityMSkillBehaviourActionSkillRecoveryPowerCorrection
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int CorrectionValuePermil { get; set; }
}
