using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponAwakenAbility))]
[PrimaryKey(nameof(WeaponAwakenAbilityId))]
public class EntityMWeaponAwakenAbility
{
    [Key(0)] public int WeaponAwakenAbilityId { get; set; }

    [Key(1)] public int AbilityId { get; set; }

    [Key(2)] public int AbilityLevel { get; set; }
}
