using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestBonusCharacterGroup
{
    public int QuestBonusCharacterGroupId { get; set; }

    public int CharacterId { get; set; }

    public int QuestBonusEffectGroupId { get; set; }

    public int QuestBonusTermGroupId { get; set; }
}
