using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAbilityBehaviourActionPassiveSkill))]
public class EntityMAbilityBehaviourActionPassiveSkill
{
    [Key(0)] public int AbilityBehaviourActionId { get; set; }

    [Key(1)] public int SkillDetailId { get; set; }
}
