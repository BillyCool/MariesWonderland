using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviourActionDamage))]
[PrimaryKey(nameof(SkillAbnormalBehaviourActionId))]
public class EntityMSkillAbnormalBehaviourActionDamage
{
    [Key(0)] public int SkillAbnormalBehaviourActionId { get; set; }

    [Key(1)] public AbnormalBehaviourDamageType AbnormalBehaviourDamageType { get; set; }

    [Key(2)] public int Power { get; set; }
}
