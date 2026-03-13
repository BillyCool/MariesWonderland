using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCostumeLotteryEffectTable : TableBase<EntityMBattleNpcCostumeLotteryEffect>
{
    private readonly Func<EntityMBattleNpcCostumeLotteryEffect, (long, string, int)> primaryIndexSelector;

    public EntityMBattleNpcCostumeLotteryEffectTable(EntityMBattleNpcCostumeLotteryEffect[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcCostumeUuid, element.SlotNumber);
    }
}
