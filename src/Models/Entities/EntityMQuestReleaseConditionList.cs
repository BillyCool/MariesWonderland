using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestReleaseConditionList
{
    public int QuestReleaseConditionListId { get; set; }

    public int QuestReleaseConditionGroupId { get; set; }

    public ConditionOperationType ConditionOperationType { get; set; }
}
