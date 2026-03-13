using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestReplayFlowRewardGroup
{
    public int QuestReplayFlowRewardGroupId { get; set; }

    public int SortOrder { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int Count { get; set; }
}
