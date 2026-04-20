using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCostumeLotteryEffectStatusUpTable : TableBase<EntityMBattleNpcCostumeLotteryEffectStatusUp>
{
    private readonly Func<EntityMBattleNpcCostumeLotteryEffectStatusUp, (long, string, StatusCalculationType)> primaryIndexSelector;

    public EntityMBattleNpcCostumeLotteryEffectStatusUpTable(EntityMBattleNpcCostumeLotteryEffectStatusUp[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcCostumeUuid, element.StatusCalculationType);
    }
}
