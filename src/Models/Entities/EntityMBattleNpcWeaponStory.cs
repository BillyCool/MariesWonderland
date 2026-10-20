using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcWeaponStory))]
[PrimaryKey(nameof(BattleNpcId), nameof(WeaponId))]
public class EntityMBattleNpcWeaponStory
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int WeaponId { get; set; }

    [Key(2)] public int ReleasedMaxStoryIndex { get; set; }
}
