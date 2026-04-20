using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalDamageMultiplyDetailAbnormal))]
public class EntityMSkillAbnormalDamageMultiplyDetailAbnormal
{
    [Key(0)] public int DamageMultiplyAbnormalDetailId { get; set; }

    [Key(1)] public int SkillDamageMultiplyAbnormalAttachedValueGroupId { get; set; }
}
