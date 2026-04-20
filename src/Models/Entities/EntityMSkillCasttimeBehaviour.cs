using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCasttimeBehaviour))]
public class EntityMSkillCasttimeBehaviour
{
    [Key(0)] public int SkillCasttimeBehaviourId { get; set; }

    [Key(1)] public SkillCasttimeBehaviourType SkillCasttimeBehaviourType { get; set; }

    [Key(2)] public int SkillCasttimeBehaviourActionId { get; set; }
}
