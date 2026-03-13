using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMAbilityLevelGroupTable : TableBase<EntityMAbilityLevelGroup>
{
    private readonly Func<EntityMAbilityLevelGroup, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMAbilityLevelGroup, int> secondaryIndexSelector;

    public EntityMAbilityLevelGroupTable(EntityMAbilityLevelGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.AbilityLevelGroupId, element.LevelLowerLimit);
        secondaryIndexSelector = element => element.AbilityLevelGroupId;
    }

    public EntityMAbilityLevelGroup FindClosestByAbilityLevelGroupIdAndLevelLowerLimit(ValueTuple<int, int> key, bool selectLower = true) =>
        FindUniqueClosestCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, selectLower);

    public RangeView<EntityMAbilityLevelGroup> FindByAbilityLevelGroupId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
