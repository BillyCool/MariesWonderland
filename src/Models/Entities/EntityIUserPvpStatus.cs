using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserPvpStatus : IUserEntity
{
    public long UserId { get; set; }

    public int StaminaMilliValue { get; set; }

    public long StaminaUpdateDatetime { get; set; }

    public int LatestRewardReceivePvpSeasonId { get; set; }

    public long LatestRewardReceivePvpWeeklyVersion { get; set; }

    public int WinStreakCount { get; set; }

    public long WinStreakCountUpdateDatetime { get; set; }

    public long LatestVersion { get; set; }
}
