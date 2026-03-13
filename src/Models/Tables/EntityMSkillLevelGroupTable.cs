using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillLevelGroupTable : TableBase<EntityMSkillLevelGroup>
{
    private readonly Func<EntityMSkillLevelGroup, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMSkillLevelGroup, int> secondaryIndexSelector;

    public EntityMSkillLevelGroupTable(EntityMSkillLevelGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.SkillLevelGroupId, element.LevelLowerLimit);
        secondaryIndexSelector = element => element.SkillLevelGroupId;
    }

    public EntityMSkillLevelGroup FindClosestBySkillLevelGroupIdAndLevelLowerLimit(ValueTuple<int, int> key, bool selectLower = true) =>
        FindUniqueClosestCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, selectLower);

    public RangeView<EntityMSkillLevelGroup> FindBySkillLevelGroupId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
