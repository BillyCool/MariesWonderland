using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleSkillFireAct))]
public class EntityMBattleSkillFireAct
{
    [Key(0)] public int BattleSkillFireActId { get; set; }

    [Key(1)] public int BattleSkillFireActConditionGroupId { get; set; }

    [Key(2)] public BattleSkillFireActConditionGroupType BattleSkillFireActConditionGroupType { get; set; }

    [Key(3)] public int BattleSkillFireActAssetId { get; set; }
}
