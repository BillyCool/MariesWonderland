namespace MariesWonderland.Models.Entities;

public class EntitySGachaRewardState : IUserEntity
{
    public long UserId { get; set; }

    public long LastRewardDrawDate { get; set; }

    public int TodaysCurrentDrawCount { get; set; }

    public int DailyMaxCount { get; set; }

    public bool RewardAvailable { get; set; } = true;
}
