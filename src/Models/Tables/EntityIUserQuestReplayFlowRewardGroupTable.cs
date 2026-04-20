using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserQuestReplayFlowRewardGroupTable : TableBase<EntityIUserQuestReplayFlowRewardGroup>
{
    private readonly Func<EntityIUserQuestReplayFlowRewardGroup, (long, int)> primaryIndexSelector;

    public EntityIUserQuestReplayFlowRewardGroupTable(EntityIUserQuestReplayFlowRewardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.QuestReplayFlowRewardGroupId);
    }

    public bool TryFindByUserIdAndQuestReplayFlowRewardGroupId(ValueTuple<long, int> key, out EntityIUserQuestReplayFlowRewardGroup result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
