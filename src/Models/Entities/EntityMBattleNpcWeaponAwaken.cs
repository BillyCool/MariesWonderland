using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcWeaponAwaken))]
[PrimaryKey(nameof(BattleNpcId), nameof(BattleNpcWeaponUuid))]
public class EntityMBattleNpcWeaponAwaken
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcWeaponUuid { get; set; }
}
