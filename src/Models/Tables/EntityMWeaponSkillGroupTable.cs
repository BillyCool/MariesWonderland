using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponSkillGroupTable : TableBase<EntityMWeaponSkillGroup>
{
    private readonly Func<EntityMWeaponSkillGroup, (int, int)> primaryIndexSelector;

    public EntityMWeaponSkillGroupTable(EntityMWeaponSkillGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponSkillGroupId, element.SlotNumber);
    }

    public EntityMWeaponSkillGroup FindByWeaponSkillGroupIdAndSlotNumber(ValueTuple<int, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);

    public RangeView<EntityMWeaponSkillGroup> FindRangeByWeaponSkillGroupIdAndSlotNumber(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
