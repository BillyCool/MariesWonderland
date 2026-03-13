using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserWeaponTable : TableBase<EntityIUserWeapon>
{
    private readonly Func<EntityIUserWeapon, (long, string)> primaryIndexSelector;

    public EntityIUserWeaponTable(EntityIUserWeapon[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserWeaponUuid);
    }

    public EntityIUserWeapon FindByUserIdAndUserWeaponUuid(ValueTuple<long, string> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key);

    public bool TryFindByUserIdAndUserWeaponUuid(ValueTuple<long, string> key, out EntityIUserWeapon result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key, out result);
}
