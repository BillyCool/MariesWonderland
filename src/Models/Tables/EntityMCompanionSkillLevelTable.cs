using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCompanionSkillLevelTable : TableBase<EntityMCompanionSkillLevel>
{
    private readonly Func<EntityMCompanionSkillLevel, int> primaryIndexSelector;

    public EntityMCompanionSkillLevelTable(EntityMCompanionSkillLevel[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CompanionLevelLowerLimit;
    }

    public EntityMCompanionSkillLevel FindClosestByCompanionLevelLowerLimit(int key, bool selectLower = true) =>
        FindUniqueClosestCore(data, primaryIndexSelector, Comparer<int>.Default, key, selectLower);

    public RangeView<EntityMCompanionSkillLevel> FindRangeByCompanionLevelLowerLimit(int min, int max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<int>.Default, min, max, ascendant);
}
