using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestTowerRewardGroup
{
    public int EventQuestTowerRewardGroupId { get; set; }

    public int SortOrder { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int Count { get; set; }
}
