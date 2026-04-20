using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestMissionGroupTable : TableBase<EntityMQuestMissionGroup>
{
    private readonly Func<EntityMQuestMissionGroup, (int, int)> primaryIndexSelector;

    public EntityMQuestMissionGroupTable(EntityMQuestMissionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestMissionGroupId, element.SortOrder);
    }

    public RangeView<EntityMQuestMissionGroup> FindRangeByQuestMissionGroupIdAndSortOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
