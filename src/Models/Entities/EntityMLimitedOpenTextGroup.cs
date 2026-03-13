using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMLimitedOpenTextGroup
{
    public int LimitedOpenTextGroupId { get; set; }

    public int SortOrder { get; set; }

    public LimitedOpenTextDisplayConditionType LimitedOpenTextDisplayConditionType { get; set; }

    public int LimitedOpenTextDisplayConditionValue { get; set; }

    public int TextAssetId { get; set; }
}
