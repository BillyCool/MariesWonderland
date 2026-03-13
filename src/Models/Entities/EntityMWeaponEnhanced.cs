using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponEnhanced
{
    public int WeaponEnhancedId { get; set; }

    public int WeaponId { get; set; }

    public int Level { get; set; }

    public int LimitBreakCount { get; set; }
}
