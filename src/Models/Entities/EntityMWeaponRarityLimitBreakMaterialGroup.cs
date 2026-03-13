using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponRarityLimitBreakMaterialGroup
{
    public RarityType RarityType { get; set; }

    public int MaterialId { get; set; }

    public int Count { get; set; }

    public int SortOrder { get; set; }
}
