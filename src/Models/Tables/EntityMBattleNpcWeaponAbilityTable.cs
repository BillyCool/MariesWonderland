using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcWeaponAbilityTable : TableBase<EntityMBattleNpcWeaponAbility>
{
    private readonly Func<EntityMBattleNpcWeaponAbility, (long, string, int)> primaryIndexSelector;

    public EntityMBattleNpcWeaponAbilityTable(EntityMBattleNpcWeaponAbility[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcWeaponUuid, element.SlotNumber);
    }
}
