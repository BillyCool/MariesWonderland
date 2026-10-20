using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCooltimeBehaviourOnExecuteDefaultSkill))]
[PrimaryKey(nameof(SkillCooltimeBehaviourActionId))]
public class EntityMSkillCooltimeBehaviourOnExecuteDefaultSkill
{
    [Key(0)] public int SkillCooltimeBehaviourActionId { get; set; }

    [Key(1)] public int SkillCooltimeAdvanceValueOnDefaultSkillGroupId { get; set; }
}
