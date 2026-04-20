using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcWeapon))]
public class EntityMBattleNpcWeapon
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcWeaponUuid { get; set; }

    [Key(2)] public int WeaponId { get; set; }

    [Key(3)] public int Level { get; set; }

    [Key(4)] public int Exp { get; set; }

    [Key(5)] public int LimitBreakCount { get; set; }

    [Key(6)] public bool IsProtected { get; set; }

    [Key(7)] public long AcquisitionDatetime { get; set; }
}
