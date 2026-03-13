using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCompanionAbilityLevelTable : TableBase<EntityMCompanionAbilityLevel>
{
    private readonly Func<EntityMCompanionAbilityLevel, int> primaryIndexSelector;

    public EntityMCompanionAbilityLevelTable(EntityMCompanionAbilityLevel[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CompanionLevelLowerLimit;
    }

    public EntityMCompanionAbilityLevel FindClosestByCompanionLevelLowerLimit(int key, bool selectLower = true) =>
        FindUniqueClosestCore(data, primaryIndexSelector, Comparer<int>.Default, key, selectLower);

    public RangeView<EntityMCompanionAbilityLevel> FindRangeByCompanionLevelLowerLimit(int min, int max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<int>.Default, min, max, ascendant);
}
