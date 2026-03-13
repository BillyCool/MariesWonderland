using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMTutorialUnlockCondition
{
    public TutorialType TutorialType { get; set; }

    public TutorialUnlockConditionType TutorialUnlockConditionType { get; set; }

    public int ConditionValue { get; set; }
}
