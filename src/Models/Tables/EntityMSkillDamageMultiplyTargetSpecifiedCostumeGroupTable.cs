using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroupTable : TableBase<EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroup>
{
    private readonly Func<EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroup, ValueTuple<int, int>> primaryIndexSelector;
    private readonly Func<EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroup, int> secondaryIndexSelector;

    public EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroupTable(EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.TargetSpecifiedCostumeGroupId, element.TargetSpecifiedCostumeGroupIndex);
        secondaryIndexSelector = element => element.TargetSpecifiedCostumeGroupId;
    }

    public RangeView<EntityMSkillDamageMultiplyTargetSpecifiedCostumeGroup> FindByTargetSpecifiedCostumeGroupId(int key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
