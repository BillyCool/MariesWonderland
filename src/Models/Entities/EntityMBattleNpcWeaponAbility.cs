using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcWeaponAbility))]
[PrimaryKey(nameof(BattleNpcId), nameof(BattleNpcWeaponUuid), nameof(SlotNumber))]
public class EntityMBattleNpcWeaponAbility
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcWeaponUuid { get; set; }

    [Key(2)] public int SlotNumber { get; set; }

    [Key(3)] public int Level { get; set; }
}
