using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleSkillFireAct
{
    public int BattleSkillFireActId { get; set; }

    public int BattleSkillFireActConditionGroupId { get; set; }

    public BattleSkillFireActConditionGroupType BattleSkillFireActConditionGroupType { get; set; }

    public int BattleSkillFireActAssetId { get; set; }
}
