using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcWeaponAwakenTable : TableBase<EntityMBattleNpcWeaponAwaken>
{
    private readonly Func<EntityMBattleNpcWeaponAwaken, (long, string)> primaryIndexSelector;

    public EntityMBattleNpcWeaponAwakenTable(EntityMBattleNpcWeaponAwaken[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcWeaponUuid);
    }
}
