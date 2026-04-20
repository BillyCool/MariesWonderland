using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalDamageMultiplyDetailCritical))]
public class EntityMSkillAbnormalDamageMultiplyDetailCritical
{
    [Key(0)] public int DamageMultiplyAbnormalDetailId { get; set; }

    [Key(1)] public bool IsCritical { get; set; }

    [Key(2)] public int DamageMultiplyCoefficientValuePermil { get; set; }
}
