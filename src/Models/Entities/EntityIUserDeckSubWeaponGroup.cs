using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(UserDeckCharacterUuid), nameof(UserWeaponUuid))]
public class EntityIUserDeckSubWeaponGroup : IUserEntity
{
    public long UserId { get; set; }

    public string UserDeckCharacterUuid { get; set; }

    public string UserWeaponUuid { get; set; }

    public int SortOrder { get; set; }

    public long LatestVersion { get; set; }
}
