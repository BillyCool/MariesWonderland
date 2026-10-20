using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(PremiumItemId))]
public class EntityIUserPremiumItem : IUserEntity
{
    public long UserId { get; set; }

    public int PremiumItemId { get; set; }

    public long AcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
