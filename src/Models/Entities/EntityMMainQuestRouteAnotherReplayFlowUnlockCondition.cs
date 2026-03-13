using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMainQuestRouteAnotherReplayFlowUnlockCondition
{
    public int MainQuestRouteId { get; set; }

    public int UnlockEvaluateConditionId { get; set; }

    public int UnlockTargetMainQuestRouteId { get; set; }
}
