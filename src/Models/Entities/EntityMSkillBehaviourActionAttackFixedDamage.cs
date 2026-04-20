using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionAttackFixedDamage))]
public class EntityMSkillBehaviourActionAttackFixedDamage
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int DamageValue { get; set; }

    [Key(2)] public bool ForceDamage { get; set; }
}
