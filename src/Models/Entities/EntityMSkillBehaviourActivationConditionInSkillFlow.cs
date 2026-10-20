using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActivationConditionInSkillFlow))]
[PrimaryKey(nameof(SkillBehaviourActivationConditionId))]
public class EntityMSkillBehaviourActivationConditionInSkillFlow
{
    [Key(0)] public int SkillBehaviourActivationConditionId { get; set; }

    [Key(1)] public int RunningSkillBehaviourType { get; set; }
}
