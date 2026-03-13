using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBigHuntScoreRewardGroupSchedule
{
    public int BigHuntScoreRewardGroupScheduleId { get; set; }

    public int GroupIndex { get; set; }

    public int BigHuntScoreRewardGroupId { get; set; }

    public long StartDatetime { get; set; }
}
