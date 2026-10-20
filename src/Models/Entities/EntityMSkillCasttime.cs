using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCasttime))]
[PrimaryKey(nameof(SkillCasttimeId))]
public class EntityMSkillCasttime
{
    [Key(0)] public int SkillCasttimeId { get; set; }

    [Key(1)] public int SkillCasttimeValue { get; set; }

    [Key(2)] public int SkillCasttimeBehaviourGroupId { get; set; }
}
