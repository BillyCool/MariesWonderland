using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcDeckLimitContentDeletedCharacterTable : TableBase<EntityMBattleNpcDeckLimitContentDeletedCharacter>
{
    private readonly Func<EntityMBattleNpcDeckLimitContentDeletedCharacter, (long, int, int)> primaryIndexSelector;

    public EntityMBattleNpcDeckLimitContentDeletedCharacterTable(EntityMBattleNpcDeckLimitContentDeletedCharacter[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcDeckNumber, element.BattleNpcDeckCharacterNumber);
    }
}
