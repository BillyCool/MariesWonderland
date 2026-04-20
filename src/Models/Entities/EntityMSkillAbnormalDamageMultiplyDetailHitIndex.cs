using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalDamageMultiplyDetailHitIndex))]
public class EntityMSkillAbnormalDamageMultiplyDetailHitIndex
{
    [Key(0)] public int DamageMultiplyAbnormalDetailId { get; set; }

    [Key(1)] public int SkillDamageMultiplyHitIndexValueGroupId { get; set; }
}
