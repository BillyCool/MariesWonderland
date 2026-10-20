using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActivationConditionActivationUpperCount))]
[PrimaryKey(nameof(SkillBehaviourActivationConditionId))]
public class EntityMSkillBehaviourActivationConditionActivationUpperCount
{
    [Key(0)] public int SkillBehaviourActivationConditionId { get; set; }

    [Key(1)] public int ActivationUpperCount { get; set; }
}
