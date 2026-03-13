using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponAwakenStatusUpGroupTable : TableBase<EntityMWeaponAwakenStatusUpGroup>
{
    private readonly Func<EntityMWeaponAwakenStatusUpGroup, (int, StatusKindType, StatusCalculationType)> primaryIndexSelector;
    private readonly Func<EntityMWeaponAwakenStatusUpGroup, int> secondaryIndexSelector;

    public EntityMWeaponAwakenStatusUpGroupTable(EntityMWeaponAwakenStatusUpGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponAwakenStatusUpGroupId, element.StatusKindType, element.StatusCalculationType);
        secondaryIndexSelector = element => element.WeaponAwakenStatusUpGroupId;
    }

    public RangeView<EntityMWeaponAwakenStatusUpGroup> FindByWeaponAwakenStatusUpGroupId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
