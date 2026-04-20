using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCooltimeBehaviour))]
public class EntityMSkillCooltimeBehaviour
{
    [Key(0)] public int SkillCooltimeBehaviourId { get; set; }

    [Key(1)] public SkillCooltimeBehaviourType SkillCooltimeBehaviourType { get; set; }

    [Key(2)] public int SkillCooltimeBehaviourActionId { get; set; }
}
