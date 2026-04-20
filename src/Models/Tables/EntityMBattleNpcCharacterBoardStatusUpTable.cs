using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCharacterBoardStatusUpTable : TableBase<EntityMBattleNpcCharacterBoardStatusUp>
{
    private readonly Func<EntityMBattleNpcCharacterBoardStatusUp, (long, int, StatusCalculationType)> primaryIndexSelector;

    public EntityMBattleNpcCharacterBoardStatusUpTable(EntityMBattleNpcCharacterBoardStatusUp[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.CharacterId, element.StatusCalculationType);
    }
}
