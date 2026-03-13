using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleCompanionSkillAiGroup
{
    public int BattleCompanionSkillAiGroupId { get; set; }

    public int Priority { get; set; }

    public BattleSchemeType BattleSchemeType { get; set; }

    public bool IsPlayerSide { get; set; }

    public SkillAiUnlockConditionValueType SkillAiUnlockConditionValueType { get; set; }

    public int SkillAiUnlockConditionValue { get; set; }

    public int BattleCompanionSkillAiId { get; set; }
}
