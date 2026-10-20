using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(UserWeaponUuid))]
public class EntityIUserWeaponAwaken : IUserEntity
{
    public long UserId { get; set; }

    public string UserWeaponUuid { get; set; }

    public long LatestVersion { get; set; }
}
