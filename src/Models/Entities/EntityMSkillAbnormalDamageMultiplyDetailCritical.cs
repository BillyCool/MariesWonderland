using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalDamageMultiplyDetailCritical))]
[PrimaryKey(nameof(DamageMultiplyAbnormalDetailId))]
public class EntityMSkillAbnormalDamageMultiplyDetailCritical
{
    [Key(0)] public int DamageMultiplyAbnormalDetailId { get; set; }

    [Key(1)] public bool IsCritical { get; set; }

    [Key(2)] public int DamageMultiplyCoefficientValuePermil { get; set; }
}
