using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMissionPassRewardGroup
{
    public int MissionPassRewardGroupId { get; set; }

    public int Level { get; set; }

    public bool IsPremium { get; set; }

    public int SortOrder { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int Count { get; set; }
}
