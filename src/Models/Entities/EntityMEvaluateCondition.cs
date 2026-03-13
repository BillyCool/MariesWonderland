using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEvaluateCondition
{
    public int EvaluateConditionId { get; set; }

    public EvaluateConditionFunctionType EvaluateConditionFunctionType { get; set; }

    public EvaluateConditionEvaluateType EvaluateConditionEvaluateType { get; set; }

    public int EvaluateConditionValueGroupId { get; set; }

    public int NameEvaluateConditionTextId { get; set; }
}
