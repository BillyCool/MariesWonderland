using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillDamageMultiplyDetailHitIndex))]
public class EntityMSkillDamageMultiplyDetailHitIndex
{
    [Key(0)] public int SkillDamageMultiplyDetailId { get; set; }

    [Key(1)] public int SkillDamageMultiplyHitIndexValueGroupId { get; set; }
}
