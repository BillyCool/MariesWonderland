using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCooltimeBehaviourOnSkillDamage))]
[PrimaryKey(nameof(SkillCooltimeBehaviourActionId))]
public class EntityMSkillCooltimeBehaviourOnSkillDamage
{
    [Key(0)] public int SkillCooltimeBehaviourActionId { get; set; }

    [Key(1)] public int SkillCooltimeAdvanceValueCalculationId { get; set; }
}
