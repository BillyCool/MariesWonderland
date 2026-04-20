using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillLevelGroup))]
public class EntityMSkillLevelGroup
{
    [Key(0)] public int SkillLevelGroupId { get; set; }

    [Key(1)] public int LevelLowerLimit { get; set; }

    [Key(2)] public int SkillDetailId { get; set; }
}
