using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserExploreScore : IUserEntity
{
    public long UserId { get; set; }

    public int ExploreId { get; set; }

    public int MaxScore { get; set; }

    public long MaxScoreUpdateDatetime { get; set; }

    public long LatestVersion { get; set; }
}
