using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserWeaponAwakenTable : TableBase<EntityIUserWeaponAwaken>
{
    private readonly Func<EntityIUserWeaponAwaken, (long, string)> primaryIndexSelector;

    public EntityIUserWeaponAwakenTable(EntityIUserWeaponAwaken[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserWeaponUuid);
    }

    public bool TryFindByUserIdAndUserWeaponUuid(ValueTuple<long, string> key, out EntityIUserWeaponAwaken result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key, out result);
}
