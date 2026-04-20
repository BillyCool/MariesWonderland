using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestReplayFlowRewardGroupTable : TableBase<EntityMQuestReplayFlowRewardGroup>
{
    private readonly Func<EntityMQuestReplayFlowRewardGroup, (int, int)> primaryIndexSelector;

    public EntityMQuestReplayFlowRewardGroupTable(EntityMQuestReplayFlowRewardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestReplayFlowRewardGroupId, element.SortOrder);
    }

    public RangeView<EntityMQuestReplayFlowRewardGroup> FindRangeByQuestReplayFlowRewardGroupIdAndSortOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
