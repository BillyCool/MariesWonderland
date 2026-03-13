using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCostumeLotteryEffectPendingTable : TableBase<EntityMBattleNpcCostumeLotteryEffectPending>
{
    private readonly Func<EntityMBattleNpcCostumeLotteryEffectPending, (long, string)> primaryIndexSelector;

    public EntityMBattleNpcCostumeLotteryEffectPendingTable(EntityMBattleNpcCostumeLotteryEffectPending[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcCostumeUuid);
    }
}
