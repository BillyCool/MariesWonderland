using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeActiveSkillGroup))]
[PrimaryKey(nameof(CostumeActiveSkillGroupId), nameof(CostumeLimitBreakCountLowerLimit))]
public class EntityMCostumeActiveSkillGroup
{
    [Key(0)] public int CostumeActiveSkillGroupId { get; set; }

    [Key(1)] public int CostumeLimitBreakCountLowerLimit { get; set; }

    [Key(2)] public int CostumeActiveSkillId { get; set; }

    [Key(3)] public int CostumeActiveSkillEnhancementMaterialId { get; set; }
}
