using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeAbilityGroupTable : TableBase<EntityMCostumeAbilityGroup>
{
    private readonly Func<EntityMCostumeAbilityGroup, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMCostumeAbilityGroup, int> secondaryIndexSelector;

    public EntityMCostumeAbilityGroupTable(EntityMCostumeAbilityGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CostumeAbilityGroupId, element.SlotNumber);
        secondaryIndexSelector = element => element.CostumeAbilityGroupId;
    }

    public RangeView<EntityMCostumeAbilityGroup> FindByCostumeAbilityGroupId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
