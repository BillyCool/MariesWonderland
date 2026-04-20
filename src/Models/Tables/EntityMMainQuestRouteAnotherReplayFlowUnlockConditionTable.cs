using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMainQuestRouteAnotherReplayFlowUnlockConditionTable : TableBase<EntityMMainQuestRouteAnotherReplayFlowUnlockCondition>
{
    private readonly Func<EntityMMainQuestRouteAnotherReplayFlowUnlockCondition, int> primaryIndexSelector;

    public EntityMMainQuestRouteAnotherReplayFlowUnlockConditionTable(EntityMMainQuestRouteAnotherReplayFlowUnlockCondition[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MainQuestRouteId;
    }

    public bool TryFindByMainQuestRouteId(int key, out EntityMMainQuestRouteAnotherReplayFlowUnlockCondition result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
