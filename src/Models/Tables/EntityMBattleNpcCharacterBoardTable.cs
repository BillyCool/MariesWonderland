using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCharacterBoardTable : TableBase<EntityMBattleNpcCharacterBoard>
{
    private readonly Func<EntityMBattleNpcCharacterBoard, (long, int)> primaryIndexSelector;

    public EntityMBattleNpcCharacterBoardTable(EntityMBattleNpcCharacterBoard[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.CharacterBoardId);
    }
}
