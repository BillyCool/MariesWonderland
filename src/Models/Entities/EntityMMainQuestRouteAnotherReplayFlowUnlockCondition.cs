using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMainQuestRouteAnotherReplayFlowUnlockCondition))]
[PrimaryKey(nameof(MainQuestRouteId))]
public class EntityMMainQuestRouteAnotherReplayFlowUnlockCondition
{
    [Key(0)] public int MainQuestRouteId { get; set; }

    [Key(1)] public int UnlockEvaluateConditionId { get; set; }

    [Key(2)] public int UnlockTargetMainQuestRouteId { get; set; }
}
