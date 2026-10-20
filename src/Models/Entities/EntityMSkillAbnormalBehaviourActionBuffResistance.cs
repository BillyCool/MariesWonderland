using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviourActionBuffResistance))]
[PrimaryKey(nameof(SkillAbnormalBehaviourActionId))]
public class EntityMSkillAbnormalBehaviourActionBuffResistance
{
    [Key(0)] public int SkillAbnormalBehaviourActionId { get; set; }

    [Key(1)] public BuffResistanceType BuffResistanceType { get; set; }

    [Key(2)] public BuffResistanceStatusKindType BuffResistanceStatusKindType { get; set; }

    [Key(3)] public int BlockProbabilityPermil { get; set; }
}
