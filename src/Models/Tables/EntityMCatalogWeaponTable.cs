using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCatalogWeaponTable : TableBase<EntityMCatalogWeapon>
{
    private readonly Func<EntityMCatalogWeapon, int> primaryIndexSelector;

    public EntityMCatalogWeaponTable(EntityMCatalogWeapon[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.WeaponId;
    }

    public EntityMCatalogWeapon FindByWeaponId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
