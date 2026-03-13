using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcWeaponNote
{
    public long BattleNpcId { get; set; }

    public int WeaponId { get; set; }

    public int MaxLevel { get; set; }

    public int MaxLimitBreakCount { get; set; }

    public long FirstAcquisitionDatetime { get; set; }
}
