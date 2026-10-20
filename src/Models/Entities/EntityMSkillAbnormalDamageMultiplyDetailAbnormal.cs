using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalDamageMultiplyDetailAbnormal))]
[PrimaryKey(nameof(DamageMultiplyAbnormalDetailId))]
public class EntityMSkillAbnormalDamageMultiplyDetailAbnormal
{
    [Key(0)] public int DamageMultiplyAbnormalDetailId { get; set; }

    [Key(1)] public int SkillDamageMultiplyAbnormalAttachedValueGroupId { get; set; }
}
