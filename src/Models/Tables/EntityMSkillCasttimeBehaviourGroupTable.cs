using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillCasttimeBehaviourGroupTable : TableBase<EntityMSkillCasttimeBehaviourGroup>
{
    private readonly Func<EntityMSkillCasttimeBehaviourGroup, (int, int)> primaryIndexSelector;

    public EntityMSkillCasttimeBehaviourGroupTable(EntityMSkillCasttimeBehaviourGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.SkillCasttimeBehaviourGroupId, element.SkillCasttimeBehaviourIndex);
    }

    public RangeView<EntityMSkillCasttimeBehaviourGroup> FindRangeBySkillCasttimeBehaviourGroupIdAndSkillCasttimeBehaviourIndex(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
