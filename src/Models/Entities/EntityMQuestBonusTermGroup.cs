using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestBonusTermGroup
{
    public int QuestBonusTermGroupId { get; set; }

    public int SortOrder { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }
}
