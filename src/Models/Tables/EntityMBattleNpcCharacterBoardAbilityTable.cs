using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCharacterBoardAbilityTable : TableBase<EntityMBattleNpcCharacterBoardAbility>
{
    private readonly Func<EntityMBattleNpcCharacterBoardAbility, (long, int, int)> primaryIndexSelector;

    public EntityMBattleNpcCharacterBoardAbilityTable(EntityMBattleNpcCharacterBoardAbility[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.CharacterId, element.AbilityId);
    }
}
