using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActivationConditionHpRatio))]
public class EntityMSkillBehaviourActivationConditionHpRatio
{
    [Key(0)] public int SkillBehaviourActivationConditionId { get; set; }

    [Key(1)] public SkillBehaviourActivationConditionHpRatioThresholdType SkillBehaviourActivationConditionHpRatioThresholdType { get; set; }

    [Key(2)] public int ThresholdRatioPermil { get; set; }
}
