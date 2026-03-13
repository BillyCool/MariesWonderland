using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMTipDisplayConditionGroup
{
    public int TipDisplayConditionGroupId { get; set; }

    public int SortOrder { get; set; }

    public TipDisplayConditionType TipDisplayConditionType { get; set; }

    public int ConditionValue { get; set; }

    public ConditionOperationType ConditionOperationType { get; set; }
}
