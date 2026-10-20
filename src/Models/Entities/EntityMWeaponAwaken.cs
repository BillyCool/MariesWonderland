using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponAwaken))]
[PrimaryKey(nameof(WeaponId))]
public class EntityMWeaponAwaken
{
    [Key(0)] public int WeaponId { get; set; }

    [Key(1)] public int WeaponAwakenEffectGroupId { get; set; }

    [Key(2)] public int WeaponAwakenMaterialGroupId { get; set; }

    [Key(3)] public int ConsumeGold { get; set; }

    [Key(4)] public int LevelLimitUp { get; set; }
}
