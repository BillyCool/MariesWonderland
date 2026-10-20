using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCooltimeBehaviourGroup))]
[PrimaryKey(nameof(SkillCooltimeBehaviourGroupId), nameof(SkillCooltimeBehaviourId))]
public class EntityMSkillCooltimeBehaviourGroup
{
    [Key(0)] public int SkillCooltimeBehaviourGroupId { get; set; }

    [Key(1)] public int SkillCooltimeBehaviourId { get; set; }
}
