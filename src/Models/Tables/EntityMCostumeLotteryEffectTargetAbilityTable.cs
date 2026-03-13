using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeLotteryEffectTargetAbilityTable : TableBase<EntityMCostumeLotteryEffectTargetAbility>
{
    private readonly Func<EntityMCostumeLotteryEffectTargetAbility, int> primaryIndexSelector;

    public EntityMCostumeLotteryEffectTargetAbilityTable(EntityMCostumeLotteryEffectTargetAbility[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CostumeLotteryEffectTargetAbilityId;
    }

    public EntityMCostumeLotteryEffectTargetAbility FindByCostumeLotteryEffectTargetAbilityId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
