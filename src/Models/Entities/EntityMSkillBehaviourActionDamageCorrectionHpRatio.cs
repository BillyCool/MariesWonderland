using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionDamageCorrectionHpRatio))]
public class EntityMSkillBehaviourActionDamageCorrectionHpRatio
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int CorrectionMaxValuePermil { get; set; }

    [Key(2)] public DamageCorrectionHpRatioType DamageCorrectionHpRatioType { get; set; }

    [Key(3)] public int ActivationThresholdHpRatioPermil { get; set; }
}
