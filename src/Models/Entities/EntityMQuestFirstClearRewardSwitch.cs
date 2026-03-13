using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestFirstClearRewardSwitch
{
    public int QuestId { get; set; }

    public int QuestFirstClearRewardGroupId { get; set; }

    public int SwitchConditionClearQuestId { get; set; }
}
