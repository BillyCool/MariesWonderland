using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMExplore
{
    public int ExploreId { get; set; }

    public int ExploreUnlockConditionId { get; set; }

    public long StartDatetime { get; set; }

    public int ConsumeItemCount { get; set; }

    public int RewardLotteryCount { get; set; }
}
