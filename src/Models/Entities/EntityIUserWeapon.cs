using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserWeapon
{
    public long UserId { get; set; }

    public string UserWeaponUuid { get; set; }

    public int WeaponId { get; set; }

    public int Level { get; set; }

    public int Exp { get; set; }

    public int LimitBreakCount { get; set; }

    public bool IsProtected { get; set; }

    public long AcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
