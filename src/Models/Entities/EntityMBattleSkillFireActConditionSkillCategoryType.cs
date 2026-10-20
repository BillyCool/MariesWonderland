using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleSkillFireActConditionSkillCategoryType))]
[PrimaryKey(nameof(BattleSkillFireActConditionId))]
public class EntityMBattleSkillFireActConditionSkillCategoryType
{
    [Key(0)] public int BattleSkillFireActConditionId { get; set; }

    [Key(1)] public SkillCategoryType SkillCategoryType { get; set; }
}
