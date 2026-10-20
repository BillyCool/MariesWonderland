using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(MovieId))]
public class EntityIUserMovie : IUserEntity
{
    public long UserId { get; set; }

    public int MovieId { get; set; }

    public long LatestViewedDatetime { get; set; }

    public long LatestVersion { get; set; }
}
