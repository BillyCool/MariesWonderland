using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCostumeLevelBonusReevaluateTable : TableBase<EntityMBattleNpcCostumeLevelBonusReevaluate>
{
    private readonly Func<EntityMBattleNpcCostumeLevelBonusReevaluate, long> primaryIndexSelector;

    public EntityMBattleNpcCostumeLevelBonusReevaluateTable(EntityMBattleNpcCostumeLevelBonusReevaluate[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BattleNpcId;
    }
}
