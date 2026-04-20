using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActivationMethod))]
public class EntityMSkillBehaviourActivationMethod
{
    [Key(0)] public int SkillBehaviourActivationMethodId { get; set; }

    [Key(1)] public ActivationMethodType ActivationMethodType { get; set; }

    [Key(2)] public int SkillBehaviourActivationConditionGroupId { get; set; }
}
