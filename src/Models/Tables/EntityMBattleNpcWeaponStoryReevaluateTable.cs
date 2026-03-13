using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcWeaponStoryReevaluateTable : TableBase<EntityMBattleNpcWeaponStoryReevaluate>
{
    private readonly Func<EntityMBattleNpcWeaponStoryReevaluate, long> primaryIndexSelector;

    public EntityMBattleNpcWeaponStoryReevaluateTable(EntityMBattleNpcWeaponStoryReevaluate[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BattleNpcId;
    }
}
