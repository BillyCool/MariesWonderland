using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponSpecificLimitBreakMaterialGroup))]
[PrimaryKey(nameof(WeaponSpecificLimitBreakMaterialGroupId), nameof(LimitBreakCountLowerLimit), nameof(MaterialId))]
public class EntityMWeaponSpecificLimitBreakMaterialGroup
{
    [Key(0)] public int WeaponSpecificLimitBreakMaterialGroupId { get; set; }

    [Key(1)] public int LimitBreakCountLowerLimit { get; set; }

    [Key(2)] public int MaterialId { get; set; }

    [Key(3)] public int Count { get; set; }

    [Key(4)] public int SortOrder { get; set; }
}
