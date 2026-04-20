using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserWeaponAwaken : IUserEntity
{
    public long UserId { get; set; }

    public string UserWeaponUuid { get; set; }

    public long LatestVersion { get; set; }
}
