using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourGroupTable : TableBase<EntityMSkillBehaviourGroup>
{
    private readonly Func<EntityMSkillBehaviourGroup, (int, int)> primaryIndexSelector;

    public EntityMSkillBehaviourGroupTable(EntityMSkillBehaviourGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.SkillBehaviourGroupId, element.SkillBehaviourId);
    }

    public RangeView<EntityMSkillBehaviourGroup> FindRangeBySkillBehaviourGroupIdAndSkillBehaviourId(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
