using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMissionReward
{
    public int MissionRewardId { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int Count { get; set; }
}
