using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestBonusEffectGroup
{
    public int QuestBonusEffectGroupId { get; set; }

    public int SortOrder { get; set; }

    public QuestBonusType QuestBonusType { get; set; }

    public int QuestBonusEffectId { get; set; }
}
