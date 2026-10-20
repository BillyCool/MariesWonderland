using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(ExploreId))]
public class EntityIUserExploreScore : IUserEntity
{
    public long UserId { get; set; }

    public int ExploreId { get; set; }

    public int MaxScore { get; set; }

    public long MaxScoreUpdateDatetime { get; set; }

    public long LatestVersion { get; set; }
}
