using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponAwakenEffectGroupTable : TableBase<EntityMWeaponAwakenEffectGroup>
{
    private readonly Func<EntityMWeaponAwakenEffectGroup, (int, int)> primaryIndexSelector;

    public EntityMWeaponAwakenEffectGroupTable(EntityMWeaponAwakenEffectGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponAwakenEffectGroupId, element.WeaponAwakenEffectType);
    }

    public EntityMWeaponAwakenEffectGroup FindByWeaponAwakenEffectGroupIdAndWeaponAwakenEffectType(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);
}
