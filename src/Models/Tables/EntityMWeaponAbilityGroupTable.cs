using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponAbilityGroupTable : TableBase<EntityMWeaponAbilityGroup>
{
    private readonly Func<EntityMWeaponAbilityGroup, (int, int)> primaryIndexSelector;

    public EntityMWeaponAbilityGroupTable(EntityMWeaponAbilityGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponAbilityGroupId, element.SlotNumber);
    }

    public EntityMWeaponAbilityGroup FindByWeaponAbilityGroupIdAndSlotNumber(ValueTuple<int, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);

    public RangeView<EntityMWeaponAbilityGroup> FindRangeByWeaponAbilityGroupIdAndSlotNumber(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
