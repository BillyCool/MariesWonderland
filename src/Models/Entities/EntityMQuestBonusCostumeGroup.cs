using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestBonusCostumeGroup
{
    public int QuestBonusCostumeGroupId { get; set; }

    public int CostumeId { get; set; }

    public int QuestBonusEffectGroupId { get; set; }

    public int QuestBonusTermGroupId { get; set; }
}
