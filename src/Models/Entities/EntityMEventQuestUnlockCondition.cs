using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestUnlockCondition
{
    public EventQuestType EventQuestType { get; set; }

    public int CharacterId { get; set; }

    public int QuestId { get; set; }

    public UnlockConditionType UnlockConditionType { get; set; }

    public int ConditionValue { get; set; }

    public int UnlockEvaluateConditionId { get; set; }
}
