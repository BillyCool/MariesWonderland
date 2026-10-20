using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(WeaponId))]
public class EntityIUserWeaponStory : IUserEntity
{
    public long UserId { get; set; }

    public int WeaponId { get; set; }

    public int ReleasedMaxStoryIndex { get; set; }

    public long LatestVersion { get; set; }
}
