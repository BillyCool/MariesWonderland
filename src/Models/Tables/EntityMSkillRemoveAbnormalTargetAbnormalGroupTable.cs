using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillRemoveAbnormalTargetAbnormalGroupTable : TableBase<EntityMSkillRemoveAbnormalTargetAbnormalGroup>
{
    private readonly Func<EntityMSkillRemoveAbnormalTargetAbnormalGroup, (int, int)> primaryIndexSelector;

    public EntityMSkillRemoveAbnormalTargetAbnormalGroupTable(EntityMSkillRemoveAbnormalTargetAbnormalGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.SkillRemoveAbnormalTargetAbnormalGroupId, element.SkillRemoveAbnormalTargetAbnormalGroupIndex);
    }

    public RangeView<EntityMSkillRemoveAbnormalTargetAbnormalGroup> FindRangeBySkillRemoveAbnormalTargetAbnormalGroupIdAndSkillRemoveAbnormalTargetAbnormalGroupIndex(
        ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
