using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcDeckSubWeaponGroupTable : TableBase<EntityMBattleNpcDeckSubWeaponGroup>
{
    private readonly Func<EntityMBattleNpcDeckSubWeaponGroup, (long, string, string)> primaryIndexSelector;

    public EntityMBattleNpcDeckSubWeaponGroupTable(EntityMBattleNpcDeckSubWeaponGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcDeckCharacterUuid, element.BattleNpcWeaponUuid);
    }
}
