using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCasttime))]
public class EntityMSkillCasttime
{
    [Key(0)] public int SkillCasttimeId { get; set; }

    [Key(1)] public int SkillCasttimeValue { get; set; }

    [Key(2)] public int SkillCasttimeBehaviourGroupId { get; set; }
}
