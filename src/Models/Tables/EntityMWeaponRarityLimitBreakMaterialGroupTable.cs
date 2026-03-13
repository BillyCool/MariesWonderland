using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponRarityLimitBreakMaterialGroupTable : TableBase<EntityMWeaponRarityLimitBreakMaterialGroup>
{
    private readonly Func<EntityMWeaponRarityLimitBreakMaterialGroup, (RarityType, int)> primaryIndexSelector;

    public EntityMWeaponRarityLimitBreakMaterialGroupTable(EntityMWeaponRarityLimitBreakMaterialGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.RarityType, element.MaterialId);
    }
}
