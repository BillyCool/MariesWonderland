using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMOverrideHitEffectConditionCriticalTable : TableBase<EntityMOverrideHitEffectConditionCritical>
{
    private readonly Func<EntityMOverrideHitEffectConditionCritical, int> primaryIndexSelector;

    public EntityMOverrideHitEffectConditionCriticalTable(EntityMOverrideHitEffectConditionCritical[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.OverrideHitEffectConditionId;
    }

    public EntityMOverrideHitEffectConditionCritical FindByOverrideHitEffectConditionId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
