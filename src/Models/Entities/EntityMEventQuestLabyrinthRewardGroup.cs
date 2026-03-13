using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestLabyrinthRewardGroup
{
    public int EventQuestLabyrinthRewardGroupId { get; set; }

    public int SortOrder { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int Count { get; set; }
}
