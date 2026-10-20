using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCasttimeBehaviourGroup))]
[PrimaryKey(nameof(SkillCasttimeBehaviourGroupId), nameof(SkillCasttimeBehaviourIndex))]
public class EntityMSkillCasttimeBehaviourGroup
{
    [Key(0)] public int SkillCasttimeBehaviourGroupId { get; set; }

    [Key(1)] public int SkillCasttimeBehaviourIndex { get; set; }

    [Key(2)] public int SkillCasttimeBehaviourId { get; set; }
}
