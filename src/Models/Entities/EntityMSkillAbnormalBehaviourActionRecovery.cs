using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviourActionRecovery))]
[PrimaryKey(nameof(SkillAbnormalBehaviourActionId))]
public class EntityMSkillAbnormalBehaviourActionRecovery
{
    [Key(0)] public int SkillAbnormalBehaviourActionId { get; set; }

    [Key(1)] public AbnormalBehaviourRecoveryType AbnormalBehaviourRecoveryType { get; set; }

    [Key(2)] public int Value { get; set; }

    [Key(3)] public int Upper { get; set; }
}
