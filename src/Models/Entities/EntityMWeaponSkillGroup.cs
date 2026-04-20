using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponSkillGroup))]
public class EntityMWeaponSkillGroup
{
    [Key(0)] public int WeaponSkillGroupId { get; set; }

    [Key(1)] public int SlotNumber { get; set; }

    [Key(2)] public int SkillId { get; set; }

    [Key(3)] public int WeaponSkillEnhancementMaterialId { get; set; }
}
