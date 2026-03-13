using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcWeapon
{
    public long BattleNpcId { get; set; }

    public string BattleNpcWeaponUuid { get; set; }

    public int WeaponId { get; set; }

    public int Level { get; set; }

    public int Exp { get; set; }

    public int LimitBreakCount { get; set; }

    public bool IsProtected { get; set; }

    public long AcquisitionDatetime { get; set; }
}
