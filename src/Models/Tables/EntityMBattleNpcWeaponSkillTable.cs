using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcWeaponSkillTable : TableBase<EntityMBattleNpcWeaponSkill>
{
    private readonly Func<EntityMBattleNpcWeaponSkill, (long, string, int)> primaryIndexSelector;

    public EntityMBattleNpcWeaponSkillTable(EntityMBattleNpcWeaponSkill[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcWeaponUuid, element.SlotNumber);
    }
}
