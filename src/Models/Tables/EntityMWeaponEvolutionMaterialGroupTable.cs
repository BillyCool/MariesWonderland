using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponEvolutionMaterialGroupTable : TableBase<EntityMWeaponEvolutionMaterialGroup>
{
    private readonly Func<EntityMWeaponEvolutionMaterialGroup, (int, int)> primaryIndexSelector;

    public EntityMWeaponEvolutionMaterialGroupTable(EntityMWeaponEvolutionMaterialGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponEvolutionMaterialGroupId, element.MaterialId);
    }
}
