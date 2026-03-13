using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserWeaponSkill
{
    public long UserId { get; set; }

    public string UserWeaponUuid { get; set; }

    public int SlotNumber { get; set; }

    public int Level { get; set; }

    public long LatestVersion { get; set; }
}
