using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserBigHuntProgressStatus : IUserEntity
{
    public long UserId { get; set; }

    public int CurrentBigHuntBossQuestId { get; set; }

    public int CurrentBigHuntQuestId { get; set; }

    public int CurrentQuestSceneId { get; set; }

    public bool IsDryRun { get; set; }

    public long LatestVersion { get; set; }
}
