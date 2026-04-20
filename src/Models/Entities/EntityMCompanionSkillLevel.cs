using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCompanionSkillLevel))]
public class EntityMCompanionSkillLevel
{
    [Key(0)] public int CompanionLevelLowerLimit { get; set; }

    [Key(1)] public int SkillLevel { get; set; }
}
