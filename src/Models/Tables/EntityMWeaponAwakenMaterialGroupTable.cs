using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponAwakenMaterialGroupTable : TableBase<EntityMWeaponAwakenMaterialGroup>
{
    private readonly Func<EntityMWeaponAwakenMaterialGroup, (int, int)> primaryIndexSelector;

    public EntityMWeaponAwakenMaterialGroupTable(EntityMWeaponAwakenMaterialGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponAwakenMaterialGroupId, element.MaterialId);
    }

    public RangeView<EntityMWeaponAwakenMaterialGroup> FindRangeByWeaponAwakenMaterialGroupIdAndMaterialId(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
