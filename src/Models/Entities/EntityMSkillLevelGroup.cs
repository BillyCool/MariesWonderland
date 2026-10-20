using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillLevelGroup))]
[PrimaryKey(nameof(SkillLevelGroupId), nameof(LevelLowerLimit))]
public class EntityMSkillLevelGroup
{
    [Key(0)] public int SkillLevelGroupId { get; set; }

    [Key(1)] public int LevelLowerLimit { get; set; }

    [Key(2)] public int SkillDetailId { get; set; }
}
