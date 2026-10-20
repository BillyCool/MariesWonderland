using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleSkillFireActConditionGroup))]
[PrimaryKey(nameof(BattleSkillFireActConditionGroupId), nameof(BattleSkillFireActConditionGroupOrder))]
public class EntityMBattleSkillFireActConditionGroup
{
    [Key(0)] public int BattleSkillFireActConditionGroupId { get; set; }

    [Key(1)] public int BattleSkillFireActConditionGroupOrder { get; set; }

    [Key(2)] public BattleSkillFireActConditionType BattleSkillFireActConditionType { get; set; }

    [Key(3)] public int BattleSkillFireActConditionId { get; set; }
}
