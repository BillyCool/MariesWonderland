using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcWeaponAwaken))]
public class EntityMBattleNpcWeaponAwaken
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcWeaponUuid { get; set; }
}
