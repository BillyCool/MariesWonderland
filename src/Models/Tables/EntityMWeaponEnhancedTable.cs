using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponEnhancedTable : TableBase<EntityMWeaponEnhanced>
{
    private readonly Func<EntityMWeaponEnhanced, int> primaryIndexSelector;

    public EntityMWeaponEnhancedTable(EntityMWeaponEnhanced[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.WeaponEnhancedId;
    }

    public bool TryFindByWeaponEnhancedId(int key, out EntityMWeaponEnhanced result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
