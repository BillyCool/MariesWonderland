using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(BigHuntScheduleId), nameof(BigHuntBossId))]
public class EntityIUserBigHuntScheduleMaxScore : IUserEntity
{
    public long UserId { get; set; }

    public int BigHuntScheduleId { get; set; }

    public int BigHuntBossId { get; set; }

    public long MaxScore { get; set; }

    public long MaxScoreUpdateDatetime { get; set; }

    public long LatestVersion { get; set; }
}
