using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionAttackCombo))]
[PrimaryKey(nameof(SkillBehaviourActionId))]
public class EntityMSkillBehaviourActionAttackCombo
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int SkillPowerCalculationId { get; set; }
}
