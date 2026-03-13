using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestReleaseConditionGroup
{
    public int QuestReleaseConditionGroupId { get; set; }

    public int SortOrder { get; set; }

    public QuestReleaseConditionType QuestReleaseConditionType { get; set; }

    public int QuestReleaseConditionId { get; set; }
}
