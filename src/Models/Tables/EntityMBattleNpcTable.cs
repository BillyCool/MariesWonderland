using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcTable : TableBase<EntityMBattleNpc>
{
    private readonly Func<EntityMBattleNpc, long> primaryIndexSelector;

    public EntityMBattleNpcTable(EntityMBattleNpc[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BattleNpcId;
    }
}
