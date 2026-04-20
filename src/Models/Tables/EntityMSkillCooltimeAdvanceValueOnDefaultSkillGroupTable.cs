using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillCooltimeAdvanceValueOnDefaultSkillGroupTable : TableBase<EntityMSkillCooltimeAdvanceValueOnDefaultSkillGroup>
{
    private readonly Func<EntityMSkillCooltimeAdvanceValueOnDefaultSkillGroup, (int, int)> primaryIndexSelector;

    public EntityMSkillCooltimeAdvanceValueOnDefaultSkillGroupTable(EntityMSkillCooltimeAdvanceValueOnDefaultSkillGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.SkillCooltimeAdvanceValueOnDefaultSkillGroupId, element.SkillHitCountLowerLimit);
    }

    public RangeView<EntityMSkillCooltimeAdvanceValueOnDefaultSkillGroup> FindRangeBySkillCooltimeAdvanceValueOnDefaultSkillGroupIdAndSkillHitCountLowerLimit(
        ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
