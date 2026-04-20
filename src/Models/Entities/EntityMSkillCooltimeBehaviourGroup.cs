using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCooltimeBehaviourGroup))]
public class EntityMSkillCooltimeBehaviourGroup
{
    [Key(0)] public int SkillCooltimeBehaviourGroupId { get; set; }

    [Key(1)] public int SkillCooltimeBehaviourId { get; set; }
}
