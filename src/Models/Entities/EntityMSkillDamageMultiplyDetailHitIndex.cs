using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillDamageMultiplyDetailHitIndex))]
[PrimaryKey(nameof(SkillDamageMultiplyDetailId))]
public class EntityMSkillDamageMultiplyDetailHitIndex
{
    [Key(0)] public int SkillDamageMultiplyDetailId { get; set; }

    [Key(1)] public int SkillDamageMultiplyHitIndexValueGroupId { get; set; }
}
