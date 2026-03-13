using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMExploreGroup
{
    public int ExploreGroupId { get; set; }

    public DifficultyType DifficultyType { get; set; }

    public int ExploreId { get; set; }
}
