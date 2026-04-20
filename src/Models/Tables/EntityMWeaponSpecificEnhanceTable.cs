using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponSpecificEnhanceTable : TableBase<EntityMWeaponSpecificEnhance>
{
    private readonly Func<EntityMWeaponSpecificEnhance, int> primaryIndexSelector;

    public EntityMWeaponSpecificEnhanceTable(EntityMWeaponSpecificEnhance[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.WeaponSpecificEnhanceId;
    }

    public EntityMWeaponSpecificEnhance FindByWeaponSpecificEnhanceId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
