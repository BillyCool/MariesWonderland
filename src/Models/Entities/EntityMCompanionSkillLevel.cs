using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCompanionSkillLevel))]
[PrimaryKey(nameof(CompanionLevelLowerLimit))]
public class EntityMCompanionSkillLevel
{
    [Key(0)] public int CompanionLevelLowerLimit { get; set; }

    [Key(1)] public int SkillLevel { get; set; }
}
