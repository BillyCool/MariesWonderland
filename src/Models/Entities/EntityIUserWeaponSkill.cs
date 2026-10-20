using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(UserWeaponUuid), nameof(SlotNumber))]
public class EntityIUserWeaponSkill : IUserEntity
{
    public long UserId { get; set; }

    public string UserWeaponUuid { get; set; }

    public int SlotNumber { get; set; }

    public int Level { get; set; }

    public long LatestVersion { get; set; }
}
