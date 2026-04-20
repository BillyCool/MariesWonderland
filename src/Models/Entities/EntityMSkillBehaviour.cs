using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviour))]
public class EntityMSkillBehaviour
{
    [Key(0)] public int SkillBehaviourId { get; set; }

    [Key(1)] public SkillBehaviourType SkillBehaviourType { get; set; }

    [Key(2)] public int SkillBehaviourActionId { get; set; }

    [Key(3)] public int SkillBehaviourActivationMethodId { get; set; }

    [Key(4)] public int SkillBehaviourAssetCalculatorId { get; set; }

    [Key(5)] public int HitRatioPermil { get; set; }

    [Key(6)] public SkillBehaviourLifetimeCalculationMethodType SkillBehaviourLifetimeCalculationMethodType { get; set; }

    [Key(7)] public int LifetimeCount { get; set; }

    [Key(8)] public int SkillTargetScopeAssetCalculatorId { get; set; }
}
