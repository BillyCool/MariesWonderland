using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule
{
    public int BigHuntWeeklyAttributeScoreRewardGroupScheduleId { get; set; }

    public AttributeType AttributeType { get; set; }

    public int GroupIndex { get; set; }

    public int BigHuntScoreRewardGroupId { get; set; }

    public long StartDatetime { get; set; }
}
