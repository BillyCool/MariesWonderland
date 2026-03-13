using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMThought
{
    public int ThoughtId { get; set; }

    public RarityType RarityType { get; set; }

    public int AbilityId { get; set; }

    public int AbilityLevel { get; set; }

    public int ThoughtAssetId { get; set; }
}
