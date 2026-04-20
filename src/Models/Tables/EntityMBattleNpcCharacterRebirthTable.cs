using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCharacterRebirthTable : TableBase<EntityMBattleNpcCharacterRebirth>
{
    private readonly Func<EntityMBattleNpcCharacterRebirth, (long, int)> primaryIndexSelector;

    public EntityMBattleNpcCharacterRebirthTable(EntityMBattleNpcCharacterRebirth[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.CharacterId);
    }
}
