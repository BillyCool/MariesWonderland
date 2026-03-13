using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestBonusCostumeSettingGroup
{
    public int QuestBonusCostumeSettingGroupId { get; set; }

    public int CostumeId { get; set; }

    public int LimitBreakCountLowerLimit { get; set; }

    public int QuestBonusEffectGroupId { get; set; }

    public int QuestBonusTermGroupId { get; set; }
}
