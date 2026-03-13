using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponSpecificLimitBreakMaterialGroup
{
    public int WeaponSpecificLimitBreakMaterialGroupId { get; set; }

    public int LimitBreakCountLowerLimit { get; set; }

    public int MaterialId { get; set; }

    public int Count { get; set; }

    public int SortOrder { get; set; }
}
