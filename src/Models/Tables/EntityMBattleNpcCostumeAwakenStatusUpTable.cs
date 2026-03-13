using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCostumeAwakenStatusUpTable : TableBase<EntityMBattleNpcCostumeAwakenStatusUp>
{
    private readonly Func<EntityMBattleNpcCostumeAwakenStatusUp, (long, string, StatusCalculationType)> primaryIndexSelector;

    public EntityMBattleNpcCostumeAwakenStatusUpTable(EntityMBattleNpcCostumeAwakenStatusUp[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcCostumeUuid, element.StatusCalculationType);
    }
}
