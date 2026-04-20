using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkill))]
public class EntityMSkill
{
    [Key(0)] public int SkillId { get; set; }

    [Key(1)] public int SkillLevelGroupId { get; set; }
}
