using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMLibraryMovieUnlockCondition
{
    public int LibraryMovieUnlockConditionId { get; set; }

    public UnlockConditionType UnlockConditionType { get; set; }

    public int ConditionValue { get; set; }
}
