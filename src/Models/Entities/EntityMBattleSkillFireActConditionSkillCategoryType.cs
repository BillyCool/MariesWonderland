using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleSkillFireActConditionSkillCategoryType))]
public class EntityMBattleSkillFireActConditionSkillCategoryType
{
    [Key(0)] public int BattleSkillFireActConditionId { get; set; }

    [Key(1)] public SkillCategoryType SkillCategoryType { get; set; }
}
