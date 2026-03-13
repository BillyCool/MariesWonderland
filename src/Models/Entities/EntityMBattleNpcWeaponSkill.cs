using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcWeaponSkill
{
    public long BattleNpcId { get; set; }

    public string BattleNpcWeaponUuid { get; set; }

    public int SlotNumber { get; set; }

    public int Level { get; set; }
}
