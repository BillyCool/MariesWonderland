using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCompanionAbilityGroupTable : TableBase<EntityMCompanionAbilityGroup>
{
    private readonly Func<EntityMCompanionAbilityGroup, (int, int)> primaryIndexSelector;

    public EntityMCompanionAbilityGroupTable(EntityMCompanionAbilityGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CompanionAbilityGroupId, element.SlotNumber);
    }

    public EntityMCompanionAbilityGroup FindByCompanionAbilityGroupIdAndSlotNumber(ValueTuple<int, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);
}
