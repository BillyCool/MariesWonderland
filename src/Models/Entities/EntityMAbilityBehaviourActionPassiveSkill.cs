using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAbilityBehaviourActionPassiveSkill))]
[PrimaryKey(nameof(AbilityBehaviourActionId))]
public class EntityMAbilityBehaviourActionPassiveSkill
{
    [Key(0)] public int AbilityBehaviourActionId { get; set; }

    [Key(1)] public int SkillDetailId { get; set; }
}
