using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestFirstClearRewardGroup
{
    public int QuestFirstClearRewardGroupId { get; set; }

    public QuestFirstClearRewardType QuestFirstClearRewardType { get; set; }

    public int SortOrder { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int Count { get; set; }

    public bool IsPickup { get; set; }
}
