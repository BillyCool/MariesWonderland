using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcDeckCharacterDressupCostumeTable : TableBase<EntityMBattleNpcDeckCharacterDressupCostume>
{
    private readonly Func<EntityMBattleNpcDeckCharacterDressupCostume, (long, string)> primaryIndexSelector;

    public EntityMBattleNpcDeckCharacterDressupCostumeTable(EntityMBattleNpcDeckCharacterDressupCostume[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcDeckCharacterUuid);
    }
}
