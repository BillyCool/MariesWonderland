using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserWeaponNote
{
    public long UserId { get; set; }

    public int WeaponId { get; set; }

    public int MaxLevel { get; set; }

    public int MaxLimitBreakCount { get; set; }

    public long FirstAcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
