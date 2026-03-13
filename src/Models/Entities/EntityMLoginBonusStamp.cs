using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMLoginBonusStamp
{
    public int LoginBonusId { get; set; }

    public int LowerPageNumber { get; set; }

    public int StampNumber { get; set; }

    public PossessionType RewardPossessionType { get; set; }

    public int RewardPossessionId { get; set; }

    public int RewardCount { get; set; }
}
