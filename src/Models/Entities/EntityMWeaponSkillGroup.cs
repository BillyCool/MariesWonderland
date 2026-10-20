using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponSkillGroup))]
[PrimaryKey(nameof(WeaponSkillGroupId), nameof(SlotNumber))]
public class EntityMWeaponSkillGroup
{
    [Key(0)] public int WeaponSkillGroupId { get; set; }

    [Key(1)] public int SlotNumber { get; set; }

    [Key(2)] public int SkillId { get; set; }

    [Key(3)] public int WeaponSkillEnhancementMaterialId { get; set; }
}
