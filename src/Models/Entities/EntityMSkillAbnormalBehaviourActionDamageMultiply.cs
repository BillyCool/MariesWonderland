using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviourActionDamageMultiply))]
[PrimaryKey(nameof(SkillAbnormalBehaviourActionId))]
public class EntityMSkillAbnormalBehaviourActionDamageMultiply
{
    [Key(0)] public int SkillAbnormalBehaviourActionId { get; set; }

    [Key(1)] public DamageMultiplyDetailType DamageMultiplyDetailType { get; set; }

    [Key(2)] public DamageMultiplyTargetType DamageMultiplyTargetType { get; set; }

    [Key(3)] public int DamageMultiplyAbnormalDetailId { get; set; }
}
