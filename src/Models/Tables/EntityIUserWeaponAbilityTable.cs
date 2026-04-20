using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserWeaponAbilityTable : TableBase<EntityIUserWeaponAbility>
{
    private readonly Func<EntityIUserWeaponAbility, (long, string, int)> primaryIndexSelector;

    public EntityIUserWeaponAbilityTable(EntityIUserWeaponAbility[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserWeaponUuid, element.SlotNumber);
    }

    public bool TryFindByUserIdAndUserWeaponUuidAndSlotNumber(ValueTuple<long, string, int> key, out EntityIUserWeaponAbility result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, string, int)>.Default, key, out result);
}
