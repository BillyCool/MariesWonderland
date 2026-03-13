using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserWeaponStory
{
    public long UserId { get; set; }

    public int WeaponId { get; set; }

    public int ReleasedMaxStoryIndex { get; set; }

    public long LatestVersion { get; set; }
}
