using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillCooltimeBehaviourGroupTable : TableBase<EntityMSkillCooltimeBehaviourGroup>
{
    private readonly Func<EntityMSkillCooltimeBehaviourGroup, (int, int)> primaryIndexSelector;

    public EntityMSkillCooltimeBehaviourGroupTable(EntityMSkillCooltimeBehaviourGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.SkillCooltimeBehaviourGroupId, element.SkillCooltimeBehaviourId);
    }

    public RangeView<EntityMSkillCooltimeBehaviourGroup> FindRangeBySkillCooltimeBehaviourGroupIdAndSkillCooltimeBehaviourId(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
