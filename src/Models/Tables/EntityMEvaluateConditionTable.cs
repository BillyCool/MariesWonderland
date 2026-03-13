using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEvaluateConditionTable : TableBase<EntityMEvaluateCondition>
{
    private readonly Func<EntityMEvaluateCondition, int> primaryIndexSelector;

    public EntityMEvaluateConditionTable(EntityMEvaluateCondition[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.EvaluateConditionId;
    }

    public EntityMEvaluateCondition FindByEvaluateConditionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
