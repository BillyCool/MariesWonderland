using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillDamageMultiplyDetailAbnormalAttached))]
public class EntityMSkillDamageMultiplyDetailAbnormalAttached
{
    [Key(0)] public int SkillDamageMultiplyDetailId { get; set; }

    [Key(1)] public int SkillDamageMultiplyAbnormalAttachedValueGroupId { get; set; }
}
