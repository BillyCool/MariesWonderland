using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(NaviCutInId))]
public class EntityIUserNaviCutIn : IUserEntity
{
    public long UserId { get; set; }

    public int NaviCutInId { get; set; }

    public long PlayDatetime { get; set; }

    public long LatestVersion { get; set; }
}
