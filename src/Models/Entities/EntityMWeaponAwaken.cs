using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponAwaken))]
public class EntityMWeaponAwaken
{
    [Key(0)] public int WeaponId { get; set; }

    [Key(1)] public int WeaponAwakenEffectGroupId { get; set; }

    [Key(2)] public int WeaponAwakenMaterialGroupId { get; set; }

    [Key(3)] public int ConsumeGold { get; set; }

    [Key(4)] public int LevelLimitUp { get; set; }
}
