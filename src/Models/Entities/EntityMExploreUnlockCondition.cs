using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMExploreUnlockCondition
{
    public int ExploreUnlockConditionId { get; set; }

    public ExploreUnlockConditionType ExploreUnlockConditionType { get; set; }

    public int ConditionValue { get; set; }
}
