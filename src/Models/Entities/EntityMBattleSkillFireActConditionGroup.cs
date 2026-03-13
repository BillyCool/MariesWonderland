using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleSkillFireActConditionGroup
{
    public int BattleSkillFireActConditionGroupId { get; set; }

    public int BattleSkillFireActConditionGroupOrder { get; set; }

    public BattleSkillFireActConditionType BattleSkillFireActConditionType { get; set; }

    public int BattleSkillFireActConditionId { get; set; }
}
