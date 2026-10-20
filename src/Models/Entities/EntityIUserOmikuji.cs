using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(OmikujiId))]
public class EntityIUserOmikuji : IUserEntity
{
    public long UserId { get; set; }

    public int OmikujiId { get; set; }

    public long LatestDrawDatetime { get; set; }

    public long LatestVersion { get; set; }
}
