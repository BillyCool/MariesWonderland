using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalDamageMultiplyDetailHitIndex))]
[PrimaryKey(nameof(DamageMultiplyAbnormalDetailId))]
public class EntityMSkillAbnormalDamageMultiplyDetailHitIndex
{
    [Key(0)] public int DamageMultiplyAbnormalDetailId { get; set; }

    [Key(1)] public int SkillDamageMultiplyHitIndexValueGroupId { get; set; }
}
