using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMissionUnlockCondition
{
    public int MissionUnlockConditionId { get; set; }

    public MissionUnlockConditionType MissionUnlockConditionType { get; set; }

    public int ConditionValue { get; set; }
}
