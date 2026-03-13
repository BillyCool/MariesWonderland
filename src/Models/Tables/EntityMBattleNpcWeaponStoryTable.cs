using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcWeaponStoryTable : TableBase<EntityMBattleNpcWeaponStory>
{
    private readonly Func<EntityMBattleNpcWeaponStory, (long, int)> primaryIndexSelector;

    public EntityMBattleNpcWeaponStoryTable(EntityMBattleNpcWeaponStory[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.WeaponId);
    }
}
