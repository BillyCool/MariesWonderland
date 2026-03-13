using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestFirstClearRewardGroupTable : TableBase<EntityMQuestFirstClearRewardGroup>
{
    private readonly Func<EntityMQuestFirstClearRewardGroup, (int, QuestFirstClearRewardType, int)> primaryIndexSelector;
    private readonly Func<EntityMQuestFirstClearRewardGroup, (int, QuestFirstClearRewardType)> secondaryIndexSelector;

    public EntityMQuestFirstClearRewardGroupTable(EntityMQuestFirstClearRewardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestFirstClearRewardGroupId, element.QuestFirstClearRewardType, element.SortOrder);
        secondaryIndexSelector = element => (element.QuestFirstClearRewardGroupId, element.QuestFirstClearRewardType);
    }

    public RangeView<EntityMQuestFirstClearRewardGroup> FindRangeByQuestFirstClearRewardGroupIdAndQuestFirstClearRewardTypeAndSortOrder(ValueTuple<int, QuestFirstClearRewardType, int> min, ValueTuple<int, QuestFirstClearRewardType, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, QuestFirstClearRewardType, int)>.Default, min, max, ascendant);

    public RangeView<EntityMQuestFirstClearRewardGroup> FindByQuestFirstClearRewardGroupIdAndQuestFirstClearRewardType(ValueTuple<int, QuestFirstClearRewardType> key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<(int, QuestFirstClearRewardType)>.Default, key);
}
