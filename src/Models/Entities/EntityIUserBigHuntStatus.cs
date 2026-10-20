using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(BigHuntBossQuestId))]
public class EntityIUserBigHuntStatus : IUserEntity
{
    public long UserId { get; set; }

    public int BigHuntBossQuestId { get; set; }

    public int DailyChallengeCount { get; set; }

    public long LatestChallengeDatetime { get; set; }

    public long LatestVersion { get; set; }
}
