using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMOverrideHitEffectConditionSkillExecutor))]
[PrimaryKey(nameof(OverrideHitEffectConditionId))]
public class EntityMOverrideHitEffectConditionSkillExecutor
{
    [Key(0)] public int OverrideHitEffectConditionId { get; set; }

    [Key(1)] public int SkillOwnerCategoryType { get; set; }
}
