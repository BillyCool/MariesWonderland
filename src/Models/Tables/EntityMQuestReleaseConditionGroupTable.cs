using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestReleaseConditionGroupTable : TableBase<EntityMQuestReleaseConditionGroup>
{
    private readonly Func<EntityMQuestReleaseConditionGroup, (int, int)> primaryIndexSelector;

    public EntityMQuestReleaseConditionGroupTable(EntityMQuestReleaseConditionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestReleaseConditionGroupId, element.SortOrder);
    }

    public RangeView<EntityMQuestReleaseConditionGroup> FindRangeByQuestReleaseConditionGroupIdAndSortOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
