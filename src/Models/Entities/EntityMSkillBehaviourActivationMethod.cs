using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActivationMethod))]
[PrimaryKey(nameof(SkillBehaviourActivationMethodId))]
public class EntityMSkillBehaviourActivationMethod
{
    [Key(0)] public int SkillBehaviourActivationMethodId { get; set; }

    [Key(1)] public ActivationMethodType ActivationMethodType { get; set; }

    [Key(2)] public int SkillBehaviourActivationConditionGroupId { get; set; }
}
