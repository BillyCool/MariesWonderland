using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestRelationMainFlow
{
    public int MainFlowQuestId { get; set; }

    public DifficultyType DifficultyType { get; set; }

    public int ReplayFlowQuestId { get; set; }

    public int SubFlowQuestId { get; set; }
}
