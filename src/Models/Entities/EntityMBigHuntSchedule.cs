using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBigHuntSchedule
{
    public int BigHuntScheduleId { get; set; }

    public long NoticeStartDatetime { get; set; }

    public long ChallengeStartDatetime { get; set; }

    public long ChallengeEndDatetime { get; set; }

    public int SeasonAssetId { get; set; }
}
