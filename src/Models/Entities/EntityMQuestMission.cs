using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestMission
{
    public int QuestMissionId { get; set; }

    public QuestMissionConditionType QuestMissionConditionType { get; set; }

    public int ConditionValue { get; set; }

    public int QuestMissionRewardId { get; set; }

    public int QuestMissionConditionValueGroupId { get; set; }
}
