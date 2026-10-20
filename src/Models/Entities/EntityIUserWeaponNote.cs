using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(WeaponId))]
public class EntityIUserWeaponNote : IUserEntity
{
    public long UserId { get; set; }

    public int WeaponId { get; set; }

    public int MaxLevel { get; set; }

    public int MaxLimitBreakCount { get; set; }

    public long FirstAcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
