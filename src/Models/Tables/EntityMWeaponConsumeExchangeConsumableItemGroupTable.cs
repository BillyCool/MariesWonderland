using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponConsumeExchangeConsumableItemGroupTable : TableBase<EntityMWeaponConsumeExchangeConsumableItemGroup>
{
    private readonly Func<EntityMWeaponConsumeExchangeConsumableItemGroup, (int, int)> primaryIndexSelector;

    public EntityMWeaponConsumeExchangeConsumableItemGroupTable(EntityMWeaponConsumeExchangeConsumableItemGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponId, element.ConsumableItemId);
    }

    public EntityMWeaponConsumeExchangeConsumableItemGroup FindByWeaponIdAndConsumableItemId(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);
}
