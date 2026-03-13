using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcWeaponTable : TableBase<EntityMBattleNpcWeapon>
{
    private readonly Func<EntityMBattleNpcWeapon, (long, string)> primaryIndexSelector;

    public EntityMBattleNpcWeaponTable(EntityMBattleNpcWeapon[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcWeaponUuid);
    }

    public EntityMBattleNpcWeapon FindByBattleNpcIdAndBattleNpcWeaponUuid(ValueTuple<long, string> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key);
}
