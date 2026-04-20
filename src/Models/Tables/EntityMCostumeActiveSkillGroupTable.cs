using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeActiveSkillGroupTable : TableBase<EntityMCostumeActiveSkillGroup>
{
    private readonly Func<EntityMCostumeActiveSkillGroup, (int, int)> primaryIndexSelector;

    public EntityMCostumeActiveSkillGroupTable(EntityMCostumeActiveSkillGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CostumeActiveSkillGroupId, element.CostumeLimitBreakCountLowerLimit);
    }

    public EntityMCostumeActiveSkillGroup FindByCostumeActiveSkillGroupIdAndCostumeLimitBreakCountLowerLimit(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);

    public EntityMCostumeActiveSkillGroup FindClosestByCostumeActiveSkillGroupIdAndCostumeLimitBreakCountLowerLimit(ValueTuple<int, int> key, bool selectLower = true) =>
        FindUniqueClosestCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, selectLower);
}
