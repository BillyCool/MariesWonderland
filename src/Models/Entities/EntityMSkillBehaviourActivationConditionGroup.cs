using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActivationConditionGroup))]
public class EntityMSkillBehaviourActivationConditionGroup
{
    [Key(0)] public int SkillBehaviourActivationConditionGroupId { get; set; }

    [Key(1)] public int ConditionCheckOrder { get; set; }

    [Key(2)] public SkillBehaviourActivationConditionType SkillBehaviourActivationConditionType { get; set; }

    [Key(3)] public int SkillBehaviourActivationConditionId { get; set; }
}
