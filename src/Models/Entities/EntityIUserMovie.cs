using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserMovie : IUserEntity
{
    public long UserId { get; set; }

    public int MovieId { get; set; }

    public long LatestViewedDatetime { get; set; }

    public long LatestVersion { get; set; }
}
