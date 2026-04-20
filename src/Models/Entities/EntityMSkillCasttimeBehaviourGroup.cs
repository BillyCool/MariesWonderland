using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCasttimeBehaviourGroup))]
public class EntityMSkillCasttimeBehaviourGroup
{
    [Key(0)] public int SkillCasttimeBehaviourGroupId { get; set; }

    [Key(1)] public int SkillCasttimeBehaviourIndex { get; set; }

    [Key(2)] public int SkillCasttimeBehaviourId { get; set; }
}
