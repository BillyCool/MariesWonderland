using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponBaseStatusTable : TableBase<EntityMWeaponBaseStatus>
{
    private readonly Func<EntityMWeaponBaseStatus, int> primaryIndexSelector;

    public EntityMWeaponBaseStatusTable(EntityMWeaponBaseStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.WeaponBaseStatusId;
    }

    public EntityMWeaponBaseStatus FindByWeaponBaseStatusId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
