using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(CageOrnamentId))]
public class EntityIUserCageOrnamentReward : IUserEntity
{
    public long UserId { get; set; }

    public int CageOrnamentId { get; set; }

    public long AcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
