using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCooltimeBehaviourOnExecuteActiveSkill))]
[PrimaryKey(nameof(SkillCooltimeBehaviourActionId))]
public class EntityMSkillCooltimeBehaviourOnExecuteActiveSkill
{
    [Key(0)] public int SkillCooltimeBehaviourActionId { get; set; }

    [Key(1)] public int SkillCooltimeAdvanceValue { get; set; }
}
