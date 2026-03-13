using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestBonus
{
    public int QuestBonusId { get; set; }

    public int QuestBonusCharacterGroupId { get; set; }

    public int QuestBonusCostumeGroupId { get; set; }

    public int QuestBonusWeaponGroupId { get; set; }

    public int QuestBonusCostumeSettingGroupId { get; set; }

    public int QuestBonusAllyCharacterId { get; set; }
}
