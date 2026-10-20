using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionAttackFixedDamage))]
[PrimaryKey(nameof(SkillBehaviourActionId))]
public class EntityMSkillBehaviourActionAttackFixedDamage
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int DamageValue { get; set; }

    [Key(2)] public bool ForceDamage { get; set; }
}
