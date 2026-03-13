using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestMissionGroup
{
    public int QuestMissionGroupId { get; set; }

    public int SortOrder { get; set; }

    public int QuestMissionId { get; set; }
}
